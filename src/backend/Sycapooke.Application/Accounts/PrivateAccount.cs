using MediatR;
using Microsoft.EntityFrameworkCore;
using Sycapooke.Application.Abstractions;
using Sycapooke.Application.Common;

namespace Sycapooke.Application.Accounts;

/// <summary>The signed-in member's own private account: nothing public, nothing about anyone else (L2-004).</summary>
public sealed record PrivateAccountDto(string DisplayName, string Email, bool EmailVerified);

public sealed record GetPrivateAccountQuery(Guid MemberId) : IRequest<Result<PrivateAccountDto>>;

public sealed class GetPrivateAccountQueryHandler(ISycapookeDbContext db) : IRequestHandler<GetPrivateAccountQuery, Result<PrivateAccountDto>>
{
    public async Task<Result<PrivateAccountDto>> Handle(GetPrivateAccountQuery query, CancellationToken cancellationToken)
    {
        var account = await db.Members.AsNoTracking()
            .Where(m => m.Id == query.MemberId && m.ClosedAt == null)
            .Select(m => new PrivateAccountDto(m.DisplayName, m.Email, m.EmailVerifiedAt != null))
            .SingleOrDefaultAsync(cancellationToken);
        return account is null ? new Error(401, "SESSION_EXPIRED", "Sign in again") : account;
    }
}

/// <summary>Renames the caller. The member comes from the session; the request carries only the name.</summary>
public sealed record UpdatePrivateAccountCommand(Guid MemberId, string? DisplayName) : IRequest<Result<PrivateAccountDto>>;

public sealed class UpdatePrivateAccountHandler(ISycapookeDbContext db) : IRequestHandler<UpdatePrivateAccountCommand, Result<PrivateAccountDto>>
{
    public async Task<Result<PrivateAccountDto>> Handle(UpdatePrivateAccountCommand command, CancellationToken cancellationToken)
    {
        var member = await db.Members.SingleOrDefaultAsync(m => m.Id == command.MemberId && m.ClosedAt == null, cancellationToken);
        if (member is null)
        {
            return new Error(401, "SESSION_EXPIRED", "Sign in again");
        }

        var name = (command.DisplayName ?? string.Empty).Trim();
        var errors = new Dictionary<string, FieldError>();
        await MemberRegistrationRules.ValidateDisplayNameAsync(db, name, errors, cancellationToken, excluding: member.Id);
        if (errors.Count > 0)
        {
            return Error.Invalid("VALIDATION_FAILED", errors);
        }

        member.Rename(name);
        await db.SaveChangesAsync(cancellationToken);
        return new PrivateAccountDto(member.DisplayName, member.Email, member.IsEmailVerified);
    }
}
