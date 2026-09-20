using System.Text.Json;

public sealed class GameStateStore
{
    private readonly GameDbContext _database;

    public GameStateStore(GameDbContext database)
    {
        _database = database;
    }

    public PlayerDataDTO GetPlayerData(string playerId)
    {
        var record = _database.PlayerData.SingleOrDefault(player => player.PlayerId == playerId);
        return record is null ? CreatePlayer(playerId) : Deserialize(record.State);
    }

    public PlayerDataDTO SavePlayerData(string playerId, PlayerDataDTO playerData)
    {
        playerData.PlayerId = playerId;
        var record = _database.PlayerData.SingleOrDefault(player => player.PlayerId == playerId);
        if (record is null)
        {
            record = new PlayerDataRecord { PlayerId = playerId };
            _database.PlayerData.Add(record);
        }

        record.State = JsonSerializer.Serialize(playerData);
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

    private static PlayerDataDTO Deserialize(string state) =>
        JsonSerializer.Deserialize<PlayerDataDTO>(state) ?? PlayerDataDTO.Create(string.Empty);
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
