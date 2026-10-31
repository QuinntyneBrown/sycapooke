using Microsoft.EntityFrameworkCore;
using Sycapooke.Application.Abstractions;
using Sycapooke.Application.Catalog;
using Sycapooke.Application.Checkouts;
using Sycapooke.Application.Listings;
using Sycapooke.Domain.Listings;
using Sycapooke.Domain.Members;
using Sycapooke.Domain.Orders;

namespace Sycapooke.Application.Orders;

/// <summary>One step of the order timeline (Paid, Shipped, Delivered, Completed) and when it was reached, if it was.</summary>
public sealed record TimelineStepDto(string Status, DateTime? At);

/// <summary>The carrier, the tracking number as stored and the carrier's public tracking page.</summary>
public sealed record TrackingDto(string Carrier, string Number, string Url);

/// <summary>Why a cancelled order was cancelled: the reason code, its label and the seller's note (L2-035).</summary>
public sealed record CancellationDto(string Reason, string ReasonLabel, string? Note);

/// <summary>The buyer's request to cancel: their message, Pending, Accepted or Declined, and when it was made (L2-035 AC2).</summary>
public sealed record CancellationRequestDto(string? Message, string Status, DateTime RequestedAt);

/// <summary>
/// One order as its seller's sale page shows it (mocks pages/sale-detail): ship-by date, the
/// buyer's address under "Ship to" (L2-033), the summary, the timeline, the tracking once shipped,
/// and the cancellation, the buyer's request to cancel and
/// whether the listing can be relisted (L2-035), and the buyer's problem with the seller's reply
/// (L2-036).
/// </summary>
public sealed record SaleDto(
    Guid Id,
    string Reference,
    string Status,
    DateTime PaidAt,
    DateTime ShipBy,
    CheckoutItemDto Item,
    string ItemPrice,
    string ShippingCost,
    string SalesTax,
    string Total,
    string BuyerDisplayName,
    AddressDto? ShipTo,
    IReadOnlyList<TimelineStepDto> Timeline,
    TrackingDto? Tracking,
    CancellationDto? Cancellation,
    CancellationRequestDto? CancellationRequest,
    bool CanRelist,
    ProblemDto? Problem,
    string? Refund);

public static class OrderViews
{
    private static readonly OrderStatus[] Steps = [OrderStatus.Paid, OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Completed];

    /// <summary>The four timeline steps, dated from the order's status history.</summary>
    public static IReadOnlyList<TimelineStepDto> Timeline(Order order) =>
        [.. Steps.Select(step => new TimelineStepDto(step.ToString(), order.StatusChanges.FirstOrDefault(c => c.To == step)?.OccurredAt))];

    public static CancellationDto? Cancellation(Order order) =>
        order.CancellationReason is { } reason ? new CancellationDto(reason.ToString(), reason.ToLabel(), order.CancellationNote) : null;

    public static CancellationRequestDto? CancellationRequest(Order order) =>
        order is { CancellationRequestStatus: { } status, CancellationRequestedAt: { } at }
            ? new CancellationRequestDto(order.CancellationRequestMessage, status.ToString(), at)
            : null;

    public static TrackingDto? Tracking(Order order) =>
        order is { Carrier: { } carrier, TrackingNumber: { } number }
            ? new TrackingDto(TrackingNumbers.Name(carrier), number, TrackingNumbers.Url(carrier, number))
            : null;

    /// <summary>The ordered item with its cover photo, as the order summary shows it.</summary>
    public static async Task<CheckoutItemDto> ItemAsync(ISycapookeDbContext db, Guid listingId, PhotoOptions photos, CancellationToken cancellationToken)
    {
        var listing = await db.Listings.AsNoTracking().SingleAsync(l => l.Id == listingId, cancellationToken);
        var cover = await db.ListingPhotos.AsNoTracking()
            .Where(p => p.ListingId == listing.Id).OrderBy(p => p.Position)
            .FirstOrDefaultAsync(cancellationToken);
        var photo = cover is null ? null : CatalogPhotos.ToDto(cover, string.Empty, photos.PublicBaseUrl.TrimEnd('/'));
        return new CheckoutItemDto(listing.Reference, listing.Title ?? string.Empty, listing.Brand ?? string.Empty, listing.Size, listing.Condition?.ToLabel() ?? string.Empty, photo);
    }

    /// <summary>Where the order's refund is: null without one, otherwise Pending or Issued.</summary>
    public static async Task<string?> RefundAsync(ISycapookeDbContext db, Order order, CancellationToken cancellationToken) =>
        (await db.Refunds.AsNoTracking().Where(r => r.PaymentId == order.PaymentId).Select(r => (RefundStatus?)r.Status).SingleOrDefaultAsync(cancellationToken))?.ToString();

    public static async Task<SaleDto> SaleAsync(ISycapookeDbContext db, Order order, PhotoOptions photos, CancellationToken cancellationToken)
    {
        var buyer = await db.Members.AsNoTracking().Where(m => m.Id == order.BuyerId).Select(m => m.ErasedAt != null ? Member.FormerCustomer : m.DisplayName).SingleAsync(cancellationToken);
        return new SaleDto(
            order.Id,
            order.Reference,
            order.Status.ToString(),
            order.PaidAt,
            order.ShipBy,
            await ItemAsync(db, order.ListingId, photos, cancellationToken),
            CheckoutMoney.Format(order.ItemPrice),
            CheckoutMoney.Format(order.ShippingCost),
            CheckoutMoney.Format(order.SalesTax),
            CheckoutMoney.Format(order.Total),
            buyer,
            AddressDto.From(order.ShippingAddress),
            Timeline(order),
            Tracking(order),
            Cancellation(order),
            CancellationRequest(order),
            order.Status == OrderStatus.Cancelled && await db.Listings.AsNoTracking().AnyAsync(l => l.Id == order.ListingId && l.Status == ListingStatus.Removed && l.RemovalReason == ListingRemovalReason.OrderCancelled, cancellationToken),
            await ProblemViews.ForOrderAsync(db, order.Id, cancellationToken),
            await RefundAsync(db, order, cancellationToken));
    }
}
