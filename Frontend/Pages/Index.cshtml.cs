using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace Frontend.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public long Points { get; set; } = 0;

        [BindProperty]
        public int ClickValue { get; set; } = 1;

        [BindProperty]
        public Dictionary<string, UpgradeData> Upgrades { get; set; } = new();

        public async Task OnGetAsync()
        {
            ApplyState(await GetStateAsync());
        }

        public async Task<IActionResult> OnPostClickAsync()
        {
            ApplyState(await SendAsync(HttpMethod.Post, "api/game/click"));
            return Page();
        }

        public async Task<IActionResult> OnPostBuyUpgradeAsync(string upgradeId)
        {
            ApplyState(await SendAsync(HttpMethod.Post, "api/game/upgrades/" + upgradeId));
            return Page();
        }

        private async Task<GameState> GetStateAsync() => await SendAsync(HttpMethod.Get, "api/game");

        private async Task<GameState> SendAsync(HttpMethod method, string path)
        {
            using var request = new HttpRequestMessage(method, path);
            request.Headers.Add("X-Game-Id", GetGameId());
            using var response = await _httpClientFactory.CreateClient("Backend").SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GameState>() ?? new GameState();
        }

        private string GetGameId()
        {
            const string cookieName = "clicker-game-id";
            if (!Request.Cookies.TryGetValue(cookieName, out var gameId) || !Guid.TryParse(gameId, out _))
            {
                gameId = Guid.NewGuid().ToString("N");
                Response.Cookies.Append(cookieName, gameId, new CookieOptions { HttpOnly = true, IsEssential = true });
            }

            return gameId;
        }

        private void ApplyState(GameState state)
        {
            Points = state.Points;
            ClickValue = state.ClickValue;
            Upgrades = state.Upgrades;
        }
    }

    public class UpgradeData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Cost { get; set; }
        public int PointsPerSecond { get; set; }
        public int ClickMultiplier { get; set; } = 1;
        public int Level { get; set; }
    }

    public class GameState
    {
        public long Points { get; set; }
        public int ClickValue { get; set; } = 1;
        public Dictionary<string, UpgradeData> Upgrades { get; set; } = new();
    }
}
