using MediatR;
using Microsoft.Extensions.Options;
using Sycapooke.Application.Orders;

namespace Sycapooke.Worker;

/// <summary>Issues refunds that could not reach the payment provider the first time.</summary>
public sealed class RefundRetryJob(IServiceScopeFactory scopes, IOptions<OrderJobOptions> options, ILogger<RefundRetryJob> logger) : BackgroundService
{
    /// <summary>One pass: returns how many refunds were issued.</summary>
    public async Task<int> RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(new RetryPendingRefundsCommand(), cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.RefundRetryInterval);
        do
        {
            try
            {
                var issued = await RunOnceAsync(stoppingToken);
                if (issued > 0)
                {
                    logger.LogInformation("Issued {Count} pending refunds", issued);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Retrying pending refunds failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
