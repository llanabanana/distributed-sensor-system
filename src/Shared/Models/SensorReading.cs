using Shared.Enums;

namespace Shared.Models;

public class SensorReading
{
    public int Id { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public DateTime Timestamp { get; set; }
    public DataQuality Quality { get; set; }
    public AlarmPriority AlarmPriority { get; set; } = AlarmPriority.None;
    public bool IsConsensus { get; set; } = false;
}
