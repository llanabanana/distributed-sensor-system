using Shared.Enums;

namespace Shared.DTOs;

public class AlarmNotificationDto
{
    public string SensorId { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public AlarmPriority Priority { get; set; }
    public DateTime Timestamp { get; set; }
}
