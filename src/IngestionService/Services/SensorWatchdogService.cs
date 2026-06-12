using IngestionService.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace IngestionService.Services;

public class SensorWatchdogService(IServiceScopeFactory scopeFactory, ILogger<SensorWatchdogService> logger)
    : BackgroundService
{
    private const int RequiredActiveSensors = 5;
    private const int InactiveThresholdSeconds = 10;
    private const int BlockDurationSeconds = 30;
    private const int CheckIntervalSeconds = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckSensors();
            await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), stoppingToken);
        }
    }

    private async Task CheckSensors()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        var allStatuses = await db.SensorStatuses.ToListAsync();

        UnblockExpiredSensors(allStatuses, now);

        MarkInactiveSensors(allStatuses, now);

        var activeSensors = allStatuses.Count(s => s.IsActive && !s.IsBlocked);
        if (activeSensors < RequiredActiveSensors)
        {
            var needed = RequiredActiveSensors - activeSensors;
            logger.LogWarning("Only {Active} active sensors, activating {Needed} more.", activeSensors, needed);
            ActivateReserveSensors(allStatuses, needed);
        }

        await db.SaveChangesAsync();
    }

    private static void MarkInactiveSensors(List<SensorStatus> statuses, DateTime now)
    {
        foreach (var s in statuses.Where(s => s.IsActive && !s.IsBlocked))
        {
            if ((now - s.LastSeen).TotalSeconds > InactiveThresholdSeconds)
            {
                s.IsActive = false;
                s.IsReserve = false;
            }
        }
    }

    private static void UnblockExpiredSensors(List<SensorStatus> statuses, DateTime now)
    {
        foreach (var s in statuses.Where(s => s.IsBlocked))
        {
            if ((now - s.BlockedAt).TotalSeconds > BlockDurationSeconds)
                s.IsBlocked = false;
        }
    }

    private static void ActivateReserveSensors(List<SensorStatus> statuses, int needed)
    {
        var reserves = statuses
            .Where(s => s.IsReserve && !s.IsBlocked)
            .OrderByDescending(s => s.LastSeen)
            .Take(needed);

        foreach (var s in reserves)
        {
            s.IsActive = true;
            s.IsReserve = false;
        }
    }
}
