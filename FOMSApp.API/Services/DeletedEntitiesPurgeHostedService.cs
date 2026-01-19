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
        // Configurable scheduling:
        // - Retention:PurgeJobInitialDelay (default 00:01:00)
        // - Retention:PurgeJobInterval (default 24:00:00)
        var initialDelay = _configuration.GetValue<TimeSpan?>("Retention:PurgeJobInitialDelay") ?? TimeSpan.FromMinutes(1);
        var interval = _configuration.GetValue<TimeSpan?>("Retention:PurgeJobInterval") ?? TimeSpan.FromHours(24);

        if (initialDelay < TimeSpan.Zero) initialDelay = TimeSpan.Zero;
        if (interval <= TimeSpan.Zero) interval = TimeSpan.FromHours(24);

        // Run once on startup after delay, then at interval.
        if (initialDelay > TimeSpan.Zero)
            await Task.Delay(initialDelay, stoppingToken);

        using var timer = new PeriodicTimer(interval);
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
        _logger.LogInformation("PurgeDeleted job starting. Retention={Retention}", retention);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var purger = scope.ServiceProvider.GetRequiredService<DeletedEntitiesPurger>();

            var result = await purger.PurgeAsync(retention, stoppingToken);
            var totalEntitiesPurged = result.VaultsPurged + result.MidpointsPurged + result.CablesPurged;
            if (totalEntitiesPurged > 0 || result.PhotoFilesDeletedSucceeded > 0)
            {
                _logger.LogWarning(
                    "=== PURGE COMPLETE === Retention={Retention}, CutoffUtc={CutoffUtc}, Vaults={Vaults}, Midpoints={Midpoints}, Cables={Cables}, PhotoFilesDeleted={Deleted}/{Attempts}",
                    result.Retention,
                    result.CutoffUtc,
                    result.VaultsPurged,
                    result.MidpointsPurged,
                    result.CablesPurged,
                    result.PhotoFilesDeletedSucceeded,
                    result.PhotoFilesDeletedAttempts);
            }
            else
            {
                _logger.LogInformation(
                    "PurgeDeleted job complete (nothing to purge). Retention={Retention}, CutoffUtc={CutoffUtc}",
                    result.Retention,
                    result.CutoffUtc);
            }
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

