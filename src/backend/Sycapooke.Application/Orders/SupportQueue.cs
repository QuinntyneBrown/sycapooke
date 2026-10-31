using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sycapooke.Application.Abstractions;
using Sycapooke.Application.Checkouts;
using Sycapooke.Application.Listings;
using Sycapooke.Domain.Orders;

namespace Sycapooke.Application.Orders;

/// <summary>
/// One request waiting for the founder (mocks pages/support): a customer's problem (reference P-…)
/// or a cancellation request (reference C-…, the order's number). <see cref="Link"/> is the page
/// that handles it; <see cref="Status"/> is Open or the problem's outcome (Refunded, Closed).
/// </summary>
public sealed record SupportRequestDto(
    string Reference,
    string Kind,
    string Label,
    CheckoutItemDto Item,
    string Total,
    string Customer,
    DateTime At,
    string Status,
    string Link);

public sealed record SupportQueueDto(IReadOnlyList<SupportRequestDto> Items, int OpenCount);

/// <summary>Customer problems and cancellation requests, newest first (ADR platform/0018).</summary>
public sealed record GetSupportQueueQuery : IRequest<SupportQueueDto>;

public sealed class GetSupportQueueQueryHandler(ISycapookeDbContext db, IOptions<PhotoOptions> photoOptions) :
    IRequestHandler<GetSupportQueueQuery, SupportQueueDto>
{
    public async Task<SupportQueueDto> Handle(GetSupportQueueQuery query, CancellationToken cancellationToken)
    {
        var items = new List<SupportRequestDto>();

        var problems = await (
            from problem in db.OrderProblems.AsNoTracking()
            join order in db.Orders.AsNoTracking() on problem.OrderId equals order.Id
            join buyer in db.Members.AsNoTracking() on order.BuyerId equals buyer.Id
            select new { problem, order.ListingId, order.Total, Buyer = buyer.DisplayName })
            .ToListAsync(cancellationToken);
        foreach (var row in problems)
        {
            var p = row.problem;
            var resolved = p.Status == ProblemStatus.Resolved;
            items.Add(new SupportRequestDto(
                p.Reference,
                "Problem",
                $"Problem: {p.Reason.ToLabel().ToLowerInvariant()}",
                await OrderViews.ItemAsync(db, row.ListingId, photoOptions.Value, cancellationToken),
                CheckoutMoney.Format(row.Total),
                row.Buyer,
                resolved ? p.ResolvedAt ?? p.OpenedAt : p.OpenedAt,
                !resolved ? "Open" : p.Outcome == ProblemOutcome.FullRefund ? "Refunded" : "Closed",
                $"/shop/problems/{p.Reference}"));
        }

        var requests = await (
            from order in db.Orders.AsNoTracking()
            join buyer in db.Members.AsNoTracking() on order.BuyerId equals buyer.Id
            where order.CancellationRequestStatus == CancellationRequestStatus.Pending
            select new { order.Id, order.Reference, order.ListingId, order.Total, order.CancellationRequestedAt, Buyer = buyer.DisplayName })
            .ToListAsync(cancellationToken);
        foreach (var row in requests)
        {
            items.Add(new SupportRequestDto(
                row.Reference,
                "CancellationRequest",
                "Cancellation request",
                await OrderViews.ItemAsync(db, row.ListingId, photoOptions.Value, cancellationToken),
                CheckoutMoney.Format(row.Total),
                row.Buyer,
                row.CancellationRequestedAt ?? DateTime.MinValue,
                "Open",
                $"/shop/sales/{row.Id}"));
        }

        var ordered = items.OrderByDescending(i => i.At).ToList();
        return new SupportQueueDto(ordered, ordered.Count(i => i.Status == "Open"));
    }
}
