namespace Sycapooke.Domain.Shop;

/// <summary>
/// The singleton binding of the shop to its founder's immutable member ID (L2-003, ADR platform/0018).
/// The primary key is fixed at <see cref="SingletonId"/>, so a second founder cannot be stored.
/// </summary>
public sealed class ShopIdentity
{
    public const int SingletonId = 1;

    private ShopIdentity()
    {
    }

    public int Id { get; private set; } = SingletonId;

    public Guid FounderMemberId { get; private set; }

    public static ShopIdentity Bind(Guid founderMemberId) => new() { FounderMemberId = founderMemberId };
}
