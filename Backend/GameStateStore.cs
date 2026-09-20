using System.Collections.Concurrent;

public sealed class GameStateStore
{
    private readonly ConcurrentDictionary<string, PlayerDataDTO> _players = new();

    public PlayerDataDTO GetPlayerData(string playerId) =>
        _players.GetOrAdd(playerId, PlayerDataDTO.Create);

    public PlayerDataDTO SavePlayerData(string playerId, PlayerDataDTO playerData)
    {
        playerData.PlayerId = playerId;
        _players[playerId] = playerData;
        return playerData;
    }

    public PlayerDataDTO Click(string playerId)
    {
        var state = GetPlayerData(playerId);
        lock (state)
        {
            state.Points += state.ClickValue;
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

            return state;
        }
    }
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
