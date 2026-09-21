using Microsoft.EntityFrameworkCore;

public sealed class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    public DbSet<PlayerDataRecord> PlayerData => Set<PlayerDataRecord>();
    public DbSet<UpgradeDataRecord> Upgrades => Set<UpgradeDataRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerDataRecord>().HasKey(player => player.PlayerId);
        modelBuilder.Entity<PlayerDataRecord>().Property(player => player.PlayerId).HasMaxLength(450);

        modelBuilder.Entity<PlayerDataRecord>()
            .HasMany(player => player.Upgrades)
            .WithOne(upgrade => upgrade.Player)
            .HasForeignKey(upgrade => upgrade.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UpgradeDataRecord>().HasKey(upgrade => new { upgrade.PlayerId, upgrade.UpgradeId });
        modelBuilder.Entity<UpgradeDataRecord>().Property(upgrade => upgrade.PlayerId).HasMaxLength(450);
        modelBuilder.Entity<UpgradeDataRecord>().Property(upgrade => upgrade.UpgradeId).HasMaxLength(100);
    }
}

public sealed class PlayerDataRecord
{
    public string PlayerId { get; set; } = string.Empty;
    public long Points { get; set; }
    public int ClickValue { get; set; } = 1;
    public List<UpgradeDataRecord> Upgrades { get; set; } = new();
}

public sealed class UpgradeDataRecord
{
    public string PlayerId { get; set; } = string.Empty;
    public string UpgradeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Cost { get; set; }
    public int PointsPerSecond { get; set; }
    public int ClickMultiplier { get; set; } = 1;
    public int Level { get; set; }
    public PlayerDataRecord Player { get; set; } = null!;
}
