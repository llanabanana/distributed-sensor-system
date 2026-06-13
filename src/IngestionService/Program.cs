using IngestionService.Data;
using IngestionService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();
builder.Services.AddScoped<AlarmDetectionService>();
builder.Services.AddHostedService<SensorWatchdogService>();
builder.Services.AddHostedService<MockDataGenerator>(); // Privremeno dok se testira
builder.Services.AddHttpClient();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
