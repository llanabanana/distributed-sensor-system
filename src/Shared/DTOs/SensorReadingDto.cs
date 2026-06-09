using Shared.Enums;

namespace Shared.DTOs;

public class SensorReadingDto
{
    public string SensorId { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public DateTime Timestamp { get; set; }
    public DataQuality Quality { get; set; }
    public long MessageId { get; set; }
}
