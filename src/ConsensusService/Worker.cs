using IngestionService.Data;

namespace ConsensusService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AppDbContext _dbContext;
    public Worker(ILogger<Worker> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            _logger.LogInformation("Consensus calculation started at: {time}", DateTimeOffset.Now);
            //await CalculateConsensusAsync(stoppingToken);
        }
    }
}
