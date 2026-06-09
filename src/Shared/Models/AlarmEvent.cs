using Shared.Enums;

namespace Shared.Models;

public class AlarmEvent
{
    public int Id { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public AlarmPriority Priority { get; set; }
    public DateTime Timestamp { get; set; }
}
