using Microsoft.EntityFrameworkCore;
using Sycapooke.Application.Abstractions;
using Sycapooke.Domain.Shop;

namespace Sycapooke.Application.Shop;

/// <summary>Deployment configuration of the shop: the one trusted founder (L2-003, ADR platform/0018).</summary>
public sealed class ShopOptions
{
    public const string Section = "Shop";

    /// <summary>The founder's immutable member ID (<c>Shop:FounderMemberId</c>); null when unset or malformed, which grants no access.</summary>
    public Guid? FounderMemberId { get; set; }
}

/// <summary>Decides whether a member is the founder. The server checks it on every protected operation (L2-010).</summary>
public interface IFounderAccess
{
    Task<bool> IsFounderAsync(Guid memberId, CancellationToken cancellationToken);
}

/// <summary>
/// The configured ID and the stored <see cref="ShopIdentity"/> must both name the member, and the
/// account must be open and verified. Email, display name, Google claims and request fields never matter.
/// </summary>
public sealed class FounderAccess(ISycapookeDbContext db, ShopOptions options) : IFounderAccess
{
    public async Task<bool> IsFounderAsync(Guid memberId, CancellationToken cancellationToken)
    {
        if (options.FounderMemberId is not { } configured || configured != memberId || memberId == Guid.Empty)
        {
            return false;
        }

        return await db.ShopIdentities.AsNoTracking().AnyAsync(s => s.Id == ShopIdentity.SingletonId && s.FounderMemberId == memberId, cancellationToken)
            && await db.Members.AsNoTracking().AnyAsync(m => m.Id == memberId && m.EmailVerifiedAt != null && m.ClosedAt == null, cancellationToken);
    }
}

public enum ProvisionOutcome
{
    Bound,
    AlreadyBound,
    UnknownMember,
    NotEligible,
    ConflictsWithStoredBinding,
}

/// <summary>
/// Binds the one founder account in a transaction-sized step run by a trusted operator
/// (<c>Sycapooke.Migrator --provision-founder</c>). Repeating the same binding is a no-op; a different
/// ID is refused. No public flow reaches it.
/// </summary>
public sealed class FounderProvisioner(ISycapookeDbContext db)
{
    public async Task<ProvisionOutcome> ProvisionAsync(Guid founderMemberId, CancellationToken cancellationToken)
    {
        var stored = await db.ShopIdentities.SingleOrDefaultAsync(s => s.Id == ShopIdentity.SingletonId, cancellationToken);
        if (stored is not null)
        {
            return stored.FounderMemberId == founderMemberId ? ProvisionOutcome.AlreadyBound : ProvisionOutcome.ConflictsWithStoredBinding;
        }

        var member = await db.Members.AsNoTracking()
            .Where(m => m.Id == founderMemberId)
            .Select(m => new { Verified = m.EmailVerifiedAt != null, Closed = m.ClosedAt != null })
            .SingleOrDefaultAsync(cancellationToken);
        if (member is null)
        {
            return ProvisionOutcome.UnknownMember;
        }

        if (!member.Verified || member.Closed)
        {
            return ProvisionOutcome.NotEligible;
        }

        db.ShopIdentities.Add(ShopIdentity.Bind(founderMemberId));
        await db.SaveChangesAsync(cancellationToken);
        return ProvisionOutcome.Bound;
    }
}
