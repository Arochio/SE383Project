using Microsoft.EntityFrameworkCore;

public sealed class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    public DbSet<PlayerDataRecord> PlayerData => Set<PlayerDataRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerDataRecord>().HasKey(player => player.PlayerId);
        modelBuilder.Entity<PlayerDataRecord>().Property(player => player.State).IsRequired();
    }
}

public sealed class PlayerDataRecord
{
    public string PlayerId { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
