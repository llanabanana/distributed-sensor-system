using Shared.Enums;

namespace Shared.Models;

public class SensorConfig
{
    public string SensorId { get; set; } = string.Empty;
    public double MinTemperature { get; set; }
    public double MaxTemperature { get; set; }
    public DataQuality Quality { get; set; } = DataQuality.Good;
    public double AlarmLowThreshold { get; set; }   // priority 1
    public double AlarmMedThreshold { get; set; }   // priority 2
    public double AlarmHighThreshold { get; set; }  // priority 3
}
