using IngestionService.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Models;

namespace IngestionService.Services;

public class AlarmDetectionService(AppDbContext db)
{
    public async Task<AlarmPriority> CheckAndRecordAlarm(string sensorId, double temperature)
    {
        var config = await db.SensorConfigs.FindAsync(sensorId);
        if (config is null)
            return AlarmPriority.None;

        var priority = GetAlarmPriority(temperature, config);

        if (priority != AlarmPriority.None)
        {
            db.AlarmEvents.Add(new AlarmEvent
            {
                SensorId = sensorId,
                Temperature = temperature,
                Priority = priority,
                Timestamp = DateTime.UtcNow
            });

            PrintAlarm(sensorId, temperature, priority);
        }

        return priority;
    }

    private static AlarmPriority GetAlarmPriority(double temperature, SensorConfig config)
    {
        if (temperature >= config.AlarmHighThreshold) return AlarmPriority.High;
        if (temperature >= config.AlarmMedThreshold)  return AlarmPriority.Medium;
        if (temperature >= config.AlarmLowThreshold)  return AlarmPriority.Low;
        return AlarmPriority.None;
    }

    private static void PrintAlarm(string sensorId, double temperature, AlarmPriority priority)
    {
        var color = priority switch
        {
            AlarmPriority.Low    => "\x1b[33m", // yellow
            AlarmPriority.Medium => "\x1b[38;5;208m", // orange
            AlarmPriority.High   => "\x1b[31m", // red
            _                    => "\x1b[0m"
        };

        Console.WriteLine($"{color}[ALARM P{(int)priority}] Sensor {sensorId}: {temperature:F2}°C\x1b[0m");
    }
}
