using MediatR;
using Microsoft.EntityFrameworkCore;
using Sycapooke.Application.Abstractions;
using Sycapooke.Application.Common;
using Sycapooke.Application.Shop;
using Sycapooke.Domain.Members;
using Sycapooke.Domain.Orders;

namespace Sycapooke.Application.Accounts;

/// <summary>
/// A customer closes their own account (L2-005). A password account proves the password; a
/// Google-only account has none and is refused with 409 GOOGLE_PROOF_REQUIRED until it signs in
/// with Google again. The founder is refused (403), and so is a customer with an order that is
/// Paid, Shipped or Delivered (409 ACCOUNT_HAS_OPEN_ORDERS, listing only their own orders).
/// </summary>
public sealed record CloseCustomerAccountCommand(Guid MemberId, string? Password) : IRequest<Result<Unit>>;

public sealed class CloseCustomerAccountHandler(
    ISycapookeDbContext db,
    IPasswordHasher hasher,
    IFounderAccess founder,
    TimeProvider clock) : IRequestHandler<CloseCustomerAccountCommand, Result<Unit>>
{
    private static readonly OrderStatus[] Open = [OrderStatus.Paid, OrderStatus.Shipped, OrderStatus.Delivered];

    public async Task<Result<Unit>> Handle(CloseCustomerAccountCommand command, CancellationToken cancellationToken)
    {
        var member = await db.Members.SingleOrDefaultAsync(m => m.Id == command.MemberId && m.ClosedAt == null, cancellationToken);
        if (member is null)
        {
            return new Error(401, "SESSION_EXPIRED", "Sign in again");
        }

        if (await founder.IsFounderAsync(member.Id, cancellationToken))
        {
            return new Error(403, "FOUNDER_CANNOT_CLOSE", "The shop's founder account can't be closed here");
        }

        if (member.PasswordHash is null)
        {
            return new Error(409, "GOOGLE_PROOF_REQUIRED", "Sign in with Google again to close this account");
        }

        if (!hasher.Verify(member.PasswordHash, command.Password ?? string.Empty))
        {
            return Error.Invalid("VALIDATION_FAILED", new Dictionary<string, FieldError>
            {
                ["password"] = new("PASSWORD_INCORRECT", "That's not your password"),
            });
        }

        var blocking = await db.Orders.AsNoTracking()
            .Where(o => o.BuyerId == member.Id && Open.Contains(o.Status))
            .OrderBy(o => o.PaidAt)
            .Select(o => new { o.Id, o.Reference, o.Status })
            .ToListAsync(cancellationToken);
        if (blocking.Count > 0)
        {
            return new Error(
                409,
                "ACCOUNT_HAS_OPEN_ORDERS",
                "Finish your open orders first",
                blocking.ToDictionary(o => o.Id.ToString(), o => new FieldError(o.Status.ToString(), o.Reference)));
        }

        var now = clock.GetUtcNow().UtcDateTime;
        member.Close(now);
        await SessionRevoker.RevokeAllAsync(db, member.Id, now, cancellationToken, save: false);
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

/// <summary>
/// Erases the personal data of every closed customer that is not yet erased: the email, the
/// display name, the saved address, the Google links and the email choices. Orders keep an
/// anonymised buyer, shown as "Former customer". Returns how many accounts it erased; repeating
/// it changes nothing.
/// </summary>
public sealed record EraseClosedCustomersCommand : IRequest<int>;

public sealed class EraseClosedCustomersHandler(ISycapookeDbContext db, TimeProvider clock) : IRequestHandler<EraseClosedCustomersCommand, int>
{
    public async Task<int> Handle(EraseClosedCustomersCommand command, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var founder = db.ShopIdentities.Select(s => s.FounderMemberId);
        var closed = await db.Members
            .Where(m => m.ClosedAt != null && m.ErasedAt == null && !founder.Contains(m.Id))
            .ToListAsync(cancellationToken);
        foreach (var member in closed)
        {
            db.ExternalLogins.RemoveRange(await db.ExternalLogins.Where(l => l.MemberId == member.Id).ToListAsync(cancellationToken));
            db.SavedAddresses.RemoveRange(await db.SavedAddresses.Where(a => a.MemberId == member.Id).ToListAsync(cancellationToken));
            db.NotificationPreferences.RemoveRange(await db.NotificationPreferences.Where(p => p.MemberId == member.Id).ToListAsync(cancellationToken));
            member.Erase(now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return closed.Count;
    }
}
