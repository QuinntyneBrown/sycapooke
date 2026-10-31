using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sycapooke.Application.Abstractions;
using Sycapooke.Application.Catalog;
using Sycapooke.Application.Common;
using Sycapooke.Application.Listings;
using Sycapooke.Application.Shop;
using Sycapooke.Domain.Listings;
using Sycapooke.Domain.Members;
using Sycapooke.Domain.Orders;

namespace Sycapooke.Application.Profiles;

/// <summary>Average to one decimal place and the count of published reviews (L2-040). Null average while there are none.</summary>
public sealed record RatingSummaryDto(decimal? Average, int Count);

/// <summary>
/// The public profile (L2-041). A dedicated allow-list of public fields: no email, phone, full name
/// or address ever reaches it (L2-054). Avatar, bio and location are null until members can edit
/// their profile (ADR reviews/0001).
/// </summary>
public sealed record PublicProfileDto(
    Guid MemberId,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    string? PublicLocation,
    DateOnly MemberSince,
    RatingSummaryDto Rating,
    int CompletedSales,
    int ActiveListingCount);

/// <summary>Joins the founder's optional city and region, for example Kingston, Jamaica.</summary>
public static class PublicLocations
{
    public static string? Join(string? city, string? region) =>
        string.Join(", ", new[] { city, region }.Where(p => !string.IsNullOrWhiteSpace(p))) is { Length: > 0 } text ? text : null;
}

/// <summary>A member's profile by display name, ignoring case.</summary>
public sealed record GetPublicProfileQuery(string DisplayName) : IRequest<Result<PublicProfileDto>>;

/// <summary>A page of the member's Active listings, newest first, 24 per page (L2-041).</summary>
public sealed record GetMemberListingsQuery(string DisplayName, int Page) : IRequest<Result<CatalogPageDto>>;

/// <summary>Finds a member who is open to the public: unknown and closed members share one 404 so neither is revealed.</summary>
internal static class PublicMembers
{
    public const int ListingsPageSize = 24;

    public static readonly Error Unavailable = Error.NotFound("MEMBER_UNAVAILABLE");

    public static Task<Member?> FindAsync(ISycapookeDbContext db, ShopOptions shop, string displayName, CancellationToken cancellationToken)
    {
        var normalized = Member.Normalize(displayName);
        var founder = shop.FounderIdOrNone;
        return db.Members.AsNoTracking().SingleOrDefaultAsync(m => m.Id == founder && m.NormalizedDisplayName == normalized && m.ClosedAt == null, cancellationToken);
    }
}

public sealed class GetPublicProfileQueryHandler(ISycapookeDbContext db, ShopOptions shop) : IRequestHandler<GetPublicProfileQuery, Result<PublicProfileDto>>
{
    public async Task<Result<PublicProfileDto>> Handle(GetPublicProfileQuery query, CancellationToken cancellationToken)
    {
        var member = await PublicMembers.FindAsync(db, shop, query.DisplayName, cancellationToken);
        if (member is null)
        {
            return PublicMembers.Unavailable;
        }

        var completedSales = await db.Orders.CountAsync(o => o.SellerId == member.Id && o.Status == OrderStatus.Completed, cancellationToken);
        var activeListings = await db.Listings.CountAsync(l => l.SellerId == member.Id && l.Status == ListingStatus.Active, cancellationToken);
        return new PublicProfileDto(
            member.Id,
            member.DisplayName,
            null,
            member.StoryBio,
            PublicLocations.Join(member.StoryCity, member.StoryRegion),
            new DateOnly(member.CreatedAt.Year, member.CreatedAt.Month, 1),
            new RatingSummaryDto(null, 0),
            completedSales,
            activeListings);
    }
}

public sealed class GetMemberListingsQueryHandler(ISycapookeDbContext db, ShopOptions shop, IOptions<PhotoOptions> photoOptions) :
    IRequestHandler<GetMemberListingsQuery, Result<CatalogPageDto>>
{
    public async Task<Result<CatalogPageDto>> Handle(GetMemberListingsQuery query, CancellationToken cancellationToken)
    {
        if (query.Page < 1)
        {
            return new Error(400, "VALIDATION_FAILED", "Page must be 1 or more");
        }

        var member = await PublicMembers.FindAsync(db, shop, query.DisplayName, cancellationToken);
        if (member is null)
        {
            return PublicMembers.Unavailable;
        }

        var active = db.Listings.AsNoTracking().Where(l => l.SellerId == member.Id && l.Status == ListingStatus.Active);
        var total = await active.CountAsync(cancellationToken);
        var listings = await active.OrderByDescending(l => l.PublishedAt).ThenByDescending(l => l.Number)
            .Skip((query.Page - 1) * PublicMembers.ListingsPageSize).Take(PublicMembers.ListingsPageSize)
            .ToListAsync(cancellationToken);
        var items = await CatalogCards.ToCardsAsync(db, listings, photoOptions.Value, cancellationToken);
        return new CatalogPageDto(items, query.Page, PublicMembers.ListingsPageSize, total);
    }
}
