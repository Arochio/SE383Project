public sealed class PlayerDataDTO
{
    public string PlayerId { get; set; } = string.Empty;
    public long Points { get; set; }
    public int ClickValue { get; set; } = 1;
    public Dictionary<string, UpgradeData> Upgrades { get; set; } = new();

    public static PlayerDataDTO Create(string playerId) => new()
    {
        PlayerId = playerId,
        Upgrades = new Dictionary<string, UpgradeData>
        {
            ["auto-clicker"] = new() { Id = "auto-clicker", Name = "Auto Clicker", Description = "Clicks automatically every second", Cost = 10, PointsPerSecond = 1 },
            ["double-click"] = new() { Id = "double-click", Name = "Double Click", Description = "Each click is worth 2 points", Cost = 25, ClickMultiplier = 2 },
            ["click-frenzy"] = new() { Id = "click-frenzy", Name = "Click Frenzy", Description = "Each click is worth 5 points", Cost = 100, ClickMultiplier = 5 },
            ["mega-farm"] = new() { Id = "mega-farm", Name = "Mega Farm", Description = "Generates 10 points per second", Cost = 500, PointsPerSecond = 10 }
        }
    };
}
