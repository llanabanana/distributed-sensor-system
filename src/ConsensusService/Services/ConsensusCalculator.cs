using IngestionService.Data;
using Shared.Enums;
using Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsensusService.Services
{
    public class ConsensusCalculator
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ConsensusCalculator> _logger;

        public ConsensusCalculator(AppDbContext context, ILogger<ConsensusCalculator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CalculateAndSaveConsensusAsync(CancellationToken token)
        {
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

            var goodData = await _context.SensorReadings
                .Where(m => m.Timestamp >= oneMinuteAgo && m.Timestamp < DateTime.UtcNow)
                .Where(m => m.Quality == DataQuality.Good)
                .AsNoTracking()
                .ToListAsync(token);

            if (goodData.Count < 2)
            {
                _logger.LogWarning("Not enough GOOD readings for consensus");
                return;
            }

            var averages = goodData
                .GroupBy(m => m.SensorId)
                .Select(g => new { SensorId = g.Key, AvgValue = g.Average(m => m.Temperature) })
                .ToList();

            var sorted = averages.Select(a => a.AvgValue).OrderBy(v => v).ToList();
            double consensusValue = sorted[sorted.Count / 2];

            // Detekcija malicioznih senzora
            double deviationThreshold = 15.0;
            bool anyMalicious = false;
            foreach (var sensorAvg in averages)
            {
                double deviation = Math.Abs(sensorAvg.AvgValue - consensusValue);
                if (deviation > deviationThreshold)
                {
                    var sensorConfig = await _context.SensorConfigs
                        .FirstOrDefaultAsync(c => c.SensorId == sensorAvg.SensorId, token);
                    if (sensorConfig != null && sensorConfig.Quality != DataQuality.Bad)
                    {
                        sensorConfig.Quality = DataQuality.Bad;
                        _logger.LogWarning("Sensor {SensorId} marked as BAD (deviation {Deviation:F2}°C)",
                            sensorAvg.SensorId, deviation);
                        anyMalicious = true;
                    }
                }
            }

            // Upis konsenzusa
            var consensusRecord = new ConsensusValue
            {
                Timestamp = oneMinuteAgo,
                Value = consensusValue,
                IsConsensus = true
            };
            await _context.ConsensusValues.AddAsync(consensusRecord, token);
            await _context.SaveChangesAsync(token);

            _logger.LogInformation("Consensus for minute {time}: {value}°C (from {count} sensors, {total} readings)",
                oneMinuteAgo, consensusValue, averages.Count, goodData.Count);
            if (anyMalicious)
                _logger.LogInformation("Some sensors were marked as BAD.");
        }
    }
}
