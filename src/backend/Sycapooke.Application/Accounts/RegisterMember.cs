using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sycapooke.Application.Abstractions;
using Sycapooke.Application.Common;
using Sycapooke.Domain.Members;

namespace Sycapooke.Application.Accounts;

/// <summary>Registers an unverified member and emails a verification link (L2-001, L2-002).</summary>
public sealed record RegisterMemberCommand(string? DisplayName, string? Email, string? Password, bool AgeConfirmed, bool TermsAccepted)
    : IRequest<Result<Guid>>;

public sealed partial class RegisterMemberCommandHandler(
    ISycapookeDbContext db,
    IPasswordHasher hasher,
    IBreachedPasswords breached,
    VerificationEmails verificationEmails,
    TimeProvider clock) : IRequestHandler<RegisterMemberCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterMemberCommand command, CancellationToken cancellationToken)
    {
        var displayName = command.DisplayName?.Trim() ?? string.Empty;
        var email = command.Email?.Trim() ?? string.Empty;
        var password = command.Password ?? string.Empty;
        var errors = new Dictionary<string, FieldError>();

        await MemberRegistrationRules.ValidateDisplayNameAsync(db, displayName, errors, cancellationToken);

        if (email.Length > 254 || !EmailPattern().IsMatch(email))
        {
            errors["email"] = new("EMAIL_FORMAT", "Enter an email address like name@example.com");
        }
        else if (await db.Members.AnyAsync(m => m.NormalizedEmail == Member.Normalize(email), cancellationToken))
        {
            errors["email"] = new("EMAIL_TAKEN", "An account with this email already exists");
        }

        PasswordRules.Validate(breached, password, "password", errors);

        MemberRegistrationRules.ValidateConsent(command.AgeConfirmed, command.TermsAccepted, errors);

        if (errors.Count > 0)
        {
            return Error.Invalid("REGISTRATION_INVALID", errors);
        }

        var member = Member.Register(displayName, email, hasher.Hash(password), clock.GetUtcNow().UtcDateTime);
        db.Members.Add(member);
        await db.SaveChangesAsync(cancellationToken);
        await verificationEmails.SendAsync(member, cancellationToken);
        return member.Id;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
