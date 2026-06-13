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
                .ToListAsync();

            if (goodData.Count < 2)
            {
                _logger.LogWarning("Not enough GOOD readings for consensus");
                return;
            }


            var averages = goodData
            .GroupBy(m => m.SensorId)
            .Select(g => g.Average(m => m.Temperature))
            .ToList();

            // median (BFT)
            var sorted = averages.OrderBy(v => v).ToList();
            double consensusValue = sorted[sorted.Count / 2];

            // save to db
            var consensusRecord = new ConsensusValue
            {
                Timestamp = oneMinuteAgo,
                Value = consensusValue,
                IsConsensus = true
            };
            await _context.ConsensusValues.AddAsync(consensusRecord, token);
            await _context.SaveChangesAsync(token);

            _logger.LogInformation("Consensus for the last minute {time}: {value}°C (from {count} sensosrs, based on {total} readings)",
                oneMinuteAgo, consensusValue, averages.Count, goodData.Count);
        
        }
    }
}
