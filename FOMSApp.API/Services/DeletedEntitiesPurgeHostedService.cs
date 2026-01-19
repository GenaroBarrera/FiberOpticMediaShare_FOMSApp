using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace FOMSApp.API.Services;

/// <summary>
/// Background job that periodically purges soft-deleted entities older than the retention window.
/// </summary>
public sealed class DeletedEntitiesPurgeHostedService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<DeletedEntitiesPurgeHostedService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<DeletedEntitiesPurgeHostedService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run once on startup after a short delay, then daily.
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnce(stoppingToken);

            // Wait until next interval
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task RunOnce(CancellationToken stoppingToken)
    {
        var retention = _configuration.GetValue<TimeSpan?>("Retention:PurgeDeletedAfter") ?? TimeSpan.FromDays(10);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var purger = scope.ServiceProvider.GetRequiredService<DeletedEntitiesPurger>();

            var result = await purger.PurgeAsync(retention, stoppingToken);
            _logger.LogInformation(
                "PurgeDeleted job complete. Retention={Retention}, CutoffUtc={CutoffUtc}, Vaults={Vaults}, Midpoints={Midpoints}, Cables={Cables}, PhotoFilesDeleted={Deleted}/{Attempts}",
                result.Retention,
                result.CutoffUtc,
                result.VaultsPurged,
                result.MidpointsPurged,
                result.CablesPurged,
                result.PhotoFilesDeletedSucceeded,
                result.PhotoFilesDeletedAttempts);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PurgeDeleted job failed.");
        }
    }
}

