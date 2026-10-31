using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sycapooke.Application.Abstractions;
using Sycapooke.Application.Common;
using Sycapooke.Application.Notifications;
using Sycapooke.Domain.Members;
using Sycapooke.Domain.Notifications;

namespace Sycapooke.Application.Accounts;

/// <summary>
/// Asks for a reset link (L2-009). The answer never depends on whether the address is registered;
/// a known address gets a link unless three were already sent in the last hour.
/// </summary>
public sealed record RequestPasswordResetCommand(string? Email) : IRequest<Unit>;

public sealed class RequestPasswordResetHandler(ISycapookeDbContext db, IEmailOutbox notifications, IOptions<AppOptions> app, TimeProvider clock)
    : IRequestHandler<RequestPasswordResetCommand, Unit>
{
    public async Task<Unit> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        var normalized = Member.Normalize(command.Email ?? string.Empty);
        var member = await db.Members.AsNoTracking()
            .Where(m => m.NormalizedEmail == normalized && m.ClosedAt == null)
            .Select(m => new { m.Id })
            .SingleOrDefaultAsync(cancellationToken);
        if (member is null)
        {
            return Unit.Value;
        }

        var now = clock.GetUtcNow().UtcDateTime;
        var since = now - PasswordResetToken.RequestWindow;
        var recent = await db.PasswordResetTokens.CountAsync(t => t.MemberId == member.Id && t.CreatedAt > since, cancellationToken);
        if (recent >= PasswordResetToken.MaxRequestsPerWindow)
        {
            return Unit.Value;
        }

        var token = SecureTokens.Create();
        db.PasswordResetTokens.Add(PasswordResetToken.Issue(member.Id, SecureTokens.Hash(token), now));
        var link = $"{app.Value.BaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";
        notifications.Enqueue(NotificationType.PasswordReset, member.Id, new PasswordResetEmail(link));
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

/// <summary>
/// Sets a new password from a reset link. The token is consumed, the password changed, every session
/// revoked and any lock cleared in one save (L2-009 AC3).
/// </summary>
public sealed record ResetPasswordCommand(string? Token, string? Password) : IRequest<Result<Unit>>;

public sealed class ResetPasswordHandler(
    ISycapookeDbContext db,
    IPasswordHasher hasher,
    IBreachedPasswords breached,
    IEmailOutbox notifications,
    TimeProvider clock) : IRequestHandler<ResetPasswordCommand, Result<Unit>>
{
    public static readonly Error Expired = new(400, "RESET_LINK_EXPIRED", "This link has expired");

    public async Task<Result<Unit>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.Token))
        {
            return Expired;
        }

        var now = clock.GetUtcNow().UtcDateTime;
        var hash = SecureTokens.Hash(command.Token);
        var token = await db.PasswordResetTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (token is null || !token.IsUsable(now))
        {
            return Expired;
        }

        var password = command.Password ?? string.Empty;
        var errors = new Dictionary<string, FieldError>();
        PasswordRules.Validate(breached, password, "password", errors);
        if (errors.Count > 0)
        {
            return Error.Invalid("PASSWORD_INVALID", errors);
        }

        var member = await db.Members.SingleOrDefaultAsync(m => m.Id == token.MemberId && m.ClosedAt == null, cancellationToken);
        if (member is null)
        {
            return Expired;
        }

        token.MarkUsed(now);
        member.ChangePassword(hasher.Hash(password));
        member.ClearSignInFailures();
        await SessionRevoker.RevokeAllAsync(db, member.Id, now, cancellationToken, save: false);
        notifications.Enqueue(NotificationType.PasswordChanged, member.Id, new PasswordChangedEmail());
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

/// <summary>
/// Changes the password of a signed-in member who proves the current one. Every other session is
/// revoked and this device gets a fresh one, so only it stays signed in.
/// </summary>
public sealed record ChangePasswordCommand(Guid MemberId, string? CurrentPassword, string? NewPassword) : IRequest<Result<SignedIn>>;

public sealed class ChangePasswordHandler(
    ISycapookeDbContext db,
    IPasswordHasher hasher,
    IBreachedPasswords breached,
    IEmailOutbox notifications,
    TimeProvider clock) : IRequestHandler<ChangePasswordCommand, Result<SignedIn>>
{
    public async Task<Result<SignedIn>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var member = await db.Members.SingleOrDefaultAsync(m => m.Id == command.MemberId && m.ClosedAt == null, cancellationToken);
        if (member is null)
        {
            return new Error(401, "SESSION_EXPIRED", "Sign in again");
        }

        var errors = new Dictionary<string, FieldError>();
        if (member.PasswordHash is null || !hasher.Verify(member.PasswordHash, command.CurrentPassword ?? string.Empty))
        {
            errors["currentPassword"] = new("CURRENT_PASSWORD_INCORRECT", "That's not your current password");
        }

        PasswordRules.Validate(breached, command.NewPassword ?? string.Empty, "newPassword", errors);
        if (errors.Count > 0)
        {
            return Error.Invalid("PASSWORD_INVALID", errors);
        }

        var now = clock.GetUtcNow().UtcDateTime;
        member.ChangePassword(hasher.Hash(command.NewPassword!));
        await SessionRevoker.RevokeAllAsync(db, member.Id, now, cancellationToken, save: false);
        notifications.Enqueue(NotificationType.PasswordChanged, member.Id, new PasswordChangedEmail());
        await db.SaveChangesAsync(cancellationToken);
        return await SessionIssuer.StartAsync(db, member.Id, now, cancellationToken);
    }
}
