using IngestionService.Data;
using Microsoft.AspNetCore.Mvc;

namespace IngestionService.Controllers;

[ApiController]
[Route("api/sensors")]
public class SensorsController(AppDbContext db) : ControllerBase
{
    [HttpPost("{sensorId}/block")]
    public async Task<IActionResult> Block(string sensorId)
    {
        var status = await db.SensorStatuses.FindAsync(sensorId);
        if (status is null)
            return NotFound();

        status.IsBlocked = true;
        status.IsActive = false;
        status.BlockedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok();
    }
}
