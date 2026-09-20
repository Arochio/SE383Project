using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/game")]
public sealed class GameController : ControllerBase
{
    private readonly GameStateStore _store;

    public GameController(GameStateStore store)
    {
        _store = store;
    }

    [HttpGet]
    public ActionResult<PlayerDataDTO> Get()
    {
        return Ok(_store.GetPlayerData(GameId()));
    }

    [HttpPost("click")]
    public ActionResult<PlayerDataDTO> Click()
    {
        return Ok(_store.Click(GameId()));
    }

    [HttpPost("upgrades/{upgradeId}")]
    public ActionResult<PlayerDataDTO> BuyUpgrade(string upgradeId)
    {
        return Ok(_store.BuyUpgrade(GameId(), upgradeId));
    }

    [HttpPut]
    public ActionResult<PlayerDataDTO> Save(PlayerDataDTO playerData)
    {
        return Ok(_store.SavePlayerData(GameId(), playerData));
    }

    private string GameId() => Request.Headers["X-Game-Id"].FirstOrDefault() ?? "anonymous";
}
