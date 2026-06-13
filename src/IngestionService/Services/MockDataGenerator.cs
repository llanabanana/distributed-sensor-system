using IngestionService.Data;
using Shared.Enums;
using Shared.Models;

namespace IngestionService.Services;

public class MockDataGenerator(IServiceScopeFactory scopeFactory, ILogger<MockDataGenerator> logger)
    : BackgroundService
{
    private readonly Random _random = new();
    private readonly string[] _sensorIds = { "sensor1", "sensor2", "sensor3", "sensor4", "sensor5" };
    private readonly double[] _minTemps = { 20, 21, 19, 22, 20 };
    private readonly double[] _maxTemps = { 25, 26, 24, 27, 25 };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await GenerateMockData();
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
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
        logger.LogInformation("Added 5 mock measurements at {time}", DateTime.UtcNow);
    }
}