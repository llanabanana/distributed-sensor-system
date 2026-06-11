using IngestionService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Models;

namespace IngestionService.Controllers;

[ApiController]
[Route("api/readings")]
public class SensorReadingsController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Ingest([FromBody] SensorReadingDto dto)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        db.SensorReadings.Add(new SensorReading
        {
            SensorId = dto.SensorId,
            Temperature = dto.Temperature,
            Timestamp = dto.Timestamp.ToUniversalTime(),
            Quality = dto.Quality,
        });

        var status = await db.SensorStatuses.FindAsync(dto.SensorId);
        if (status is null)
            db.SensorStatuses.Add(new SensorStatus
            {
                SensorId = dto.SensorId,
                IsActive = true,
                IsBlocked = false,
                LastSeen = DateTime.UtcNow
            });
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
