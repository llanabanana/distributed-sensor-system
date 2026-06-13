using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace IngestionService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<AlarmEvent> AlarmEvents => Set<AlarmEvent>();
    public DbSet<SensorStatus> SensorStatuses => Set<SensorStatus>();
    public DbSet<SensorConfig> SensorConfigs => Set<SensorConfig>();
    public DbSet<ConsensusValue> ConsensusValues => Set<ConsensusValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorReading>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SensorId).IsRequired().HasMaxLength(50);
            e.Property(x => x.Temperature).IsRequired();
            e.Property(x => x.Timestamp).IsRequired();
            e.HasIndex(x => x.SensorId);
            e.HasIndex(x => x.Timestamp);
        });

        modelBuilder.Entity<AlarmEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SensorId).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.Timestamp);
        });

        modelBuilder.Entity<SensorStatus>(e =>
        {
            e.HasKey(x => x.SensorId);
            e.Property(x => x.SensorId).HasMaxLength(50);
        });

        modelBuilder.Entity<SensorConfig>(e =>
        {
            e.HasKey(x => x.SensorId);
            e.Property(x => x.SensorId).HasMaxLength(50);
        });

        modelBuilder.Entity<ConsensusValue>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Timestamp).IsRequired();
            e.Property(x => x.Value).IsRequired();
            e.Property(x => x.IsConsensus).IsRequired();
            e.HasIndex(x => x.Timestamp); 
        });
    }
}
