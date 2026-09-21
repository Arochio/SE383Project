using Microsoft.EntityFrameworkCore;

public sealed class GameStateStore
{
    private readonly GameDbContext _database;

    public GameStateStore(GameDbContext database)
    {
        _database = database;
    }

    public PlayerDataDTO GetPlayerData(string playerId)
    {
        var record = _database.PlayerData
            .Include(player => player.Upgrades)
            .SingleOrDefault(player => player.PlayerId == playerId);
        return record is null ? CreatePlayer(playerId) : ToDto(record);
    }

    public PlayerDataDTO SavePlayerData(string playerId, PlayerDataDTO playerData)
    {
        playerData.PlayerId = playerId;
        var record = _database.PlayerData
            .Include(player => player.Upgrades)
            .SingleOrDefault(player => player.PlayerId == playerId);
        if (record is null)
        {
            record = new PlayerDataRecord { PlayerId = playerId, Upgrades = new List<UpgradeDataRecord>() };
            _database.PlayerData.Add(record);
        }

        record.Points = playerData.Points;
        record.ClickValue = playerData.ClickValue;

        foreach (var existingUpgrade in record.Upgrades
                     .Where(upgrade => !playerData.Upgrades.ContainsKey(upgrade.UpgradeId))
                     .ToList())
        {
            _database.Upgrades.Remove(existingUpgrade);
        }

        foreach (var upgrade in playerData.Upgrades.Values)
        {
            var upgradeRecord = record.Upgrades.SingleOrDefault(item => item.UpgradeId == upgrade.Id);
            if (upgradeRecord is null)
            {
                upgradeRecord = new UpgradeDataRecord { PlayerId = playerId, UpgradeId = upgrade.Id };
                record.Upgrades.Add(upgradeRecord);
            }

            upgradeRecord.Name = upgrade.Name;
            upgradeRecord.Description = upgrade.Description;
            upgradeRecord.Cost = upgrade.Cost;
            upgradeRecord.PointsPerSecond = upgrade.PointsPerSecond;
            upgradeRecord.ClickMultiplier = upgrade.ClickMultiplier;
            upgradeRecord.Level = upgrade.Level;
        }

        _database.SaveChanges();
        return playerData;
    }

    public PlayerDataDTO Click(string playerId)
    {
        var state = GetPlayerData(playerId);
        lock (state)
        {
            state.Points += state.ClickValue;
            SavePlayerData(playerId, state);
            return state;
        }
    }

    public PlayerDataDTO BuyUpgrade(string playerId, string upgradeId)
    {
        var state = GetPlayerData(playerId);
        lock (state)
        {
            if (state.Upgrades.TryGetValue(upgradeId, out var upgrade) && state.Points >= upgrade.Cost)
            {
                state.Points -= upgrade.Cost;
                upgrade.Level++;
                if (upgradeId == "double-click")
                    state.ClickValue = 2;
                else if (upgradeId == "click-frenzy")
                    state.ClickValue = 5;
                upgrade.Cost = (int)(upgrade.Cost * 1.15);
            }

            SavePlayerData(playerId, state);
            return state;
        }
    }

    private PlayerDataDTO CreatePlayer(string playerId)
    {
        var state = PlayerDataDTO.Create(playerId);
        SavePlayerData(playerId, state);
        return state;
    }

    private static PlayerDataDTO ToDto(PlayerDataRecord record) => new()
    {
        PlayerId = record.PlayerId,
        Points = record.Points,
        ClickValue = record.ClickValue,
        Upgrades = record.Upgrades.ToDictionary(
            upgrade => upgrade.UpgradeId,
            upgrade => new UpgradeData
            {
                Id = upgrade.UpgradeId,
                Name = upgrade.Name,
                Description = upgrade.Description,
                Cost = upgrade.Cost,
                PointsPerSecond = upgrade.PointsPerSecond,
                ClickMultiplier = upgrade.ClickMultiplier,
                Level = upgrade.Level
            })
    };
}

public sealed class UpgradeData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Cost { get; set; }
    public int PointsPerSecond { get; set; }
    public int ClickMultiplier { get; set; } = 1;
    public int Level { get; set; }
}
