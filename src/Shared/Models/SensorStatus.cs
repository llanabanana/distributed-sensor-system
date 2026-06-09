namespace Shared.Models;

public class SensorStatus
{
    public string SensorId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime LastSeen { get; set; }
}
