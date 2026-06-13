using ConsensusService;
using ConsensusService.Services;
using IngestionService.Data;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Consensus");

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<ConsensusCalculator>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
