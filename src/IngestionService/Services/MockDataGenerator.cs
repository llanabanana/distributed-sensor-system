using IngestionService.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Models;

namespace IngestionService.Services;

// Class used for testing until SensorSimulator is implemented
public class MockDataGenerator
    : BackgroundService
{
    private readonly Random _random = new();
    private readonly string[] _sensorIds = { "sensor1", "sensor2", "sensor3", "sensor4", "sensor5" };
    private readonly double[] _minTemps = { 20, 21, 19, 22, 100 };
    private readonly double[] _maxTemps = { 25, 26, 24, 27, 150 };
    private bool _configsSeeded = false;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MockDataGenerator> _logger;

    public MockDataGenerator(IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, ILogger<MockDataGenerator> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Init sensorConfigs (once)
        //await SeedSensorConfigsAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await GenerateMockData();
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    // initialize mock sensorConfigs in the database
    private async Task SeedSensorConfigsAsync()
    {
        if (_configsSeeded) return;

        using var scope = _scopeFactory.CreateScope();
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
            }
        }

        await db.SaveChangesAsync();
        _configsSeeded = true;
    }

    private async Task GenerateMockData()
{
    var client = _httpClientFactory.CreateClient();
    for (int i = 0; i < _sensorIds.Length; i++)
    {
        // generating five random sensor readings 
        var sensorId = _sensorIds[i];
        double temp = _minTemps[i] + _random.NextDouble() * (_maxTemps[i] - _minTemps[i]);
        SensorReading readingDto = new SensorReading
        {
            SensorId = sensorId,
            Temperature = temp,
            Timestamp = DateTime.UtcNow,
            Quality = DataQuality.Good,
            AlarmPriority = AlarmPriority.None
        };
        
        // sending readings to the sensor reading controller
        var response = await client.PostAsJsonAsync("http://localhost:5147/api/readings", readingDto);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to send reading for {SensorId}: {StatusCode}", sensorId, response.StatusCode);
        }
    }
   
    _logger.LogInformation("Sent 5 mock readings at {time} (sensor5 malicious: {temp5:F2}°C)",
        DateTime.UtcNow, _minTemps[4] + _random.NextDouble() * (_maxTemps[4] - _minTemps[4]));
}
}