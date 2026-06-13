using IngestionService.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Models;

namespace IngestionService.Services;

public class MockDataGenerator(IServiceScopeFactory scopeFactory, ILogger<MockDataGenerator> logger)
    : BackgroundService
{
    private readonly Random _random = new();
    private readonly string[] _sensorIds = { "sensor1", "sensor2", "sensor3", "sensor4", "sensor5" };
    private readonly double[] _minTemps = { 20, 21, 19, 22, 100 };
    private readonly double[] _maxTemps = { 25, 26, 24, 27, 150 };
    private bool _configsSeeded = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Seeding konfiguracija (jednom)
        await SeedSensorConfigsAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await GenerateMockData();
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task SeedSensorConfigsAsync()
    {
        if (_configsSeeded) return;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var sensorId in _sensorIds)
        {
            bool exists = await db.SensorConfigs.AnyAsync(c => c.SensorId == sensorId);
            if (!exists)
            {
                var config = new SensorConfig
                {
                    SensorId = sensorId,
                    MinTemperature = 0,         // nije kritično za test
                    MaxTemperature = 200,       // dovoljno visoko da obuhvati i maliciozne vrednosti
                    Quality = DataQuality.Good,
                    AlarmLowThreshold = 30,     // priority 1
                    AlarmMedThreshold = 35,     // priority 2
                    AlarmHighThreshold = 40     // priority 3
                };
                await db.SensorConfigs.AddAsync(config);
                logger.LogInformation("Seeded SensorConfig for {SensorId} (Quality = Good)", sensorId);
            }
        }

        await db.SaveChangesAsync();
        _configsSeeded = true;
    }

    private async Task GenerateMockData()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        for (int i = 0; i < _sensorIds.Length; i++)
        {
            var sensorId = _sensorIds[i];
            double temp = _minTemps[i] + _random.NextDouble() * (_maxTemps[i] - _minTemps[i]);
            var reading = new SensorReading
            {
                SensorId = sensorId,
                Temperature = temp,
                Timestamp = DateTime.UtcNow,
                AlarmPriority = AlarmPriority.None,
                Quality = DataQuality.Good,
                IsConsensus = false
            };
            await db.SensorReadings.AddAsync(reading);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Added 5 mock measurements at {time} (sensor5 malicious: {temp5:F2}°C)",
            DateTime.UtcNow, _minTemps[4] + _random.NextDouble() * (_maxTemps[4] - _minTemps[4]));
    }
}