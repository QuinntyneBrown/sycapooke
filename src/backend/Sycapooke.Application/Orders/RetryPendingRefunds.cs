using MediatR;
using Microsoft.EntityFrameworkCore;
using Sycapooke.Application.Abstractions;
using Sycapooke.Domain.Orders;

namespace Sycapooke.Application.Orders;

/// <summary>
/// Issues every refund that is still Pending, for example after the payment provider was unreachable.
/// The refund keeps its ID, so the provider treats a repeated call as the same refund (ADR orders/0005).
/// </summary>
public sealed record RetryPendingRefundsCommand : IRequest<int>;

public sealed class RetryPendingRefundsHandler(ISycapookeDbContext db, IPaymentProvider provider, OrderCancellation refunds) :
    IRequestHandler<RetryPendingRefundsCommand, int>
{
    public async Task<int> Handle(RetryPendingRefundsCommand command, CancellationToken cancellationToken)
    {
        var pending = await db.Refunds.Where(r => r.Status == RefundStatus.Pending).Select(r => r.Id).ToListAsync(cancellationToken);
        var issued = 0;
        foreach (var id in pending)
        {
            var refund = await db.Refunds.SingleAsync(r => r.Id == id, cancellationToken);
            var payment = await db.Payments.SingleAsync(p => p.Id == refund.PaymentId, cancellationToken);
            var order = await db.Orders.SingleOrDefaultAsync(o => o.PaymentId == payment.Id, cancellationToken);
            if (order is not null)
            {
                await refunds.IssueRefundAsync(order, refund, payment, cancellationToken);
            }
            else
            {
                try
                {
                    refund.Issued(await provider.RefundAsync(payment.ProviderPaymentId!, refund.Amount, refund.Id, cancellationToken));
                    await db.SaveChangesAsync(cancellationToken);
                }
                catch (HttpRequestException)
                {
                    // Still Pending: the next pass tries again.
                }
            }

            if (refund.Status == RefundStatus.Issued)
            {
                issued++;
            }
        }

        return issued;
    }
}
