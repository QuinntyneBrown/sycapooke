using MediatR;
using Microsoft.Extensions.Options;
using Sycapooke.Application.Accounts;

namespace Sycapooke.Worker;

/// <summary>Erases the personal data of closed customer accounts (L2-005).</summary>
public sealed class EraseClosedCustomersJob(IServiceScopeFactory scopes, IOptions<OrderJobOptions> options, ILogger<EraseClosedCustomersJob> logger) : BackgroundService
{
    /// <summary>One pass: returns how many accounts it erased.</summary>
    public async Task<int> RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(new EraseClosedCustomersCommand(), cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.EraseInterval);
        do
        {
            try
            {
                var erased = await RunOnceAsync(stoppingToken);
                if (erased > 0)
                {
                    logger.LogInformation("Erased {Count} closed customer accounts", erased);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Erasing closed customers failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
