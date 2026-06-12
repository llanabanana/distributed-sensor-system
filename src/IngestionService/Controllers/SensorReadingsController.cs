using IngestionService.Data;
using IngestionService.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Models;

namespace IngestionService.Controllers;

[ApiController]
[Route("api/readings")]
public class SensorReadingsController(AppDbContext db, AlarmDetectionService alarmService) : ControllerBase
{
    private const int RequiredActiveSensors = 5;

    [HttpPost]
    public async Task<IActionResult> Ingest([FromBody] SensorReadingDto dto)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var priority = await alarmService.CheckAndRecordAlarm(dto.SensorId, dto.Temperature);

        db.SensorReadings.Add(new SensorReading
        {
            SensorId = dto.SensorId,
            Temperature = dto.Temperature,
            Timestamp = dto.Timestamp.ToUniversalTime(),
            Quality = dto.Quality,
            AlarmPriority = priority
        });

        var status = await db.SensorStatuses.FindAsync(dto.SensorId);
        if (status is null)
        {
            var activeCount = db.SensorStatuses.Count(s => s.IsActive && !s.IsBlocked);
            var shouldBeActive = activeCount < RequiredActiveSensors;
            db.SensorStatuses.Add(new SensorStatus
            {
                SensorId = dto.SensorId,
                IsActive = shouldBeActive,
                IsReserve = !shouldBeActive,
                IsBlocked = false,
                LastSeen = DateTime.UtcNow
            });
        }
        else
        {
            status.LastSeen = DateTime.UtcNow;
            status.IsActive = true;
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok();
    }
}
