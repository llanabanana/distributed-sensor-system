using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;
using Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AlarmController : ControllerBase
{
    private readonly IHubContext<AlarmHub> _hubContext;
    private readonly ILogger<AlarmController> _logger;

    public AlarmController(IHubContext<AlarmHub> hubContext, ILogger<AlarmController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpPost("notify")]
    public async Task<IActionResult> Notify([FromBody] AlarmNotificationDto alarm)
    {
        // Log recieving a message from IngestionService
        _logger.LogWarning("Alarm received from IngestionService: Sensor {SensorId}, Temp {Temperature}, Priority {Priority}",
            alarm.SensorId, alarm.Temperature, alarm.Priority);

        // Sending notification to all clients
        await _hubContext.Clients.All.SendAsync("ReceiveAlarm", alarm);

        return Ok(new { message = "Alarm forwarded to clients" });
    }
}