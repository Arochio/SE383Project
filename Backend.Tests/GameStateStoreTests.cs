using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class GameStateStoreTests
{
    [Fact]
    public void CreatePlayer_ContainsAllDefaultUpgrades()
    {
        var player = PlayerDataDTO.Create("player-1");

        Assert.Equal("player-1", player.PlayerId);
        Assert.Equal(4, player.Upgrades.Count);
        Assert.Equal(1, player.Upgrades["auto-clicker"].PointsPerSecond);
        Assert.Equal(10, player.Upgrades["mega-farm"].PointsPerSecond);
    }

    [Fact]
    public void SavePlayerData_PersistsPlayerAndUpgradeState()
    {
        using var context = CreateContext();
        var store = new GameStateStore(context);
        var state = PlayerDataDTO.Create("player-2");
        state.Points = 42;
        state.Upgrades["auto-clicker"].Level = 3;

        store.SavePlayerData("player-2", state);
        var loaded = store.GetPlayerData("player-2");

        Assert.Equal(42, loaded.Points);
        Assert.Equal(3, loaded.Upgrades["auto-clicker"].Level);
    }

    [Fact]
    public void Click_AddsCurrentClickValue()
    {
        using var context = CreateContext();
        var store = new GameStateStore(context);
        var state = PlayerDataDTO.Create("player-3");
        state.Points = 10;
        state.Upgrades["double-click"].Level = 1;
        store.SavePlayerData("player-3", state);

        var result = store.Click("player-3");

        Assert.Equal(12, result.Points);
    }

    [Fact]
    public void BuyUpgrade_WhenUnaffordable_DoesNotChangeUpgrade()
    {
        using var context = CreateContext();
        var store = new GameStateStore(context);
        var state = PlayerDataDTO.Create("player-4");
        state.Points = 0;
        store.SavePlayerData("player-4", state);

        var result = store.BuyUpgrade("player-4", "double-click");

        Assert.Equal(0, result.Points);
        Assert.Equal(0, result.Upgrades["double-click"].Level);
        Assert.Equal(25, result.Upgrades["double-click"].Cost);
    }

    [Fact]
    public void BuyUpgrade_IncreasesLevelAndCost()
    {
        using var context = CreateContext();
        var store = new GameStateStore(context);
        var state = PlayerDataDTO.Create("player-5");
        state.Points = 100;
        store.SavePlayerData("player-5", state);

        var result = store.BuyUpgrade("player-5", "double-click");

        Assert.Equal(75, result.Points);
        Assert.Equal(1, result.Upgrades["double-click"].Level);
        Assert.Equal(28, result.Upgrades["double-click"].Cost);
    }

    [Fact]
    public void BuyUpgrade_CombinesMultiplierGroupsByLevel()
    {
        using var context = CreateContext();
        var store = new GameStateStore(context);
        var state = PlayerDataDTO.Create("player-6");
        state.Points = 10000;
        store.SavePlayerData("player-6", state);

        store.BuyUpgrade("player-6", "double-click");
        store.BuyUpgrade("player-6", "double-click");
        store.BuyUpgrade("player-6", "click-frenzy");

        var result = store.GetPlayerData("player-6");

        Assert.Equal((2 + 2) * 5, result.ClickValue);
    }

    [Fact]
    public void BuyUpgrade_AutoClickerDoesNotChangeClickMultiplier()
    {
        using var context = CreateContext();
        var store = new GameStateStore(context);
        var state = PlayerDataDTO.Create("player-7");
        state.Points = 100;
        store.SavePlayerData("player-7", state);

        var result = store.BuyUpgrade("player-7", "auto-clicker");

        Assert.Equal(1, result.ClickValue);
        Assert.Equal(1, result.Upgrades["auto-clicker"].Level);
    }

    private static GameDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }
}
