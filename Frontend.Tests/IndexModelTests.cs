using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Xunit;

public sealed class IndexModelTests
{
    [Fact]
    public async Task OnGetAsync_LoadsStateFromBackend()
    {
        var state = CreateState(points: 25, clickValue: 3);
        var handler = new StateHandler(state);
        var model = CreateModel(handler);

        await model.OnGetAsync();

        Assert.Equal(25, model.Points);
        Assert.Equal(3, model.ClickValue);
        Assert.Equal("auto-clicker", model.Upgrades["auto-clicker"].Id);
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
    }

    [Fact]
    public async Task OnGetAsync_CreatesGameCookieWhenMissing()
    {
        var model = CreateModel(new StateHandler(CreateState()));

        await model.OnGetAsync();

        Assert.Contains("clicker-game-id", model.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task OnGetStateAsync_ReturnsJsonResult()
    {
        var state = CreateState(points: 12);
        var model = CreateModel(new StateHandler(state));

        var result = await model.OnGetStateAsync();

        var json = Assert.IsType<JsonResult>(result);
        var returnedState = Assert.IsType<GameState>(json.Value);
        Assert.Equal(12, returnedState.Points);
    }

    [Fact]
    public async Task OnPostClickAsync_SendsClickRequestAndAppliesState()
    {
        var handler = new StateHandler(CreateState(points: 9, clickValue: 4));
        var model = CreateModel(handler);

        var result = await model.OnPostClickAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("http://backend/api/game/click", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(9, model.Points);
        Assert.Equal(4, model.ClickValue);
    }

    [Fact]
    public async Task OnPostBuyUpgradeAsync_SendsUpgradeIdInRequestPath()
    {
        var handler = new StateHandler(CreateState());
        var model = CreateModel(handler);

        await model.OnPostBuyUpgradeAsync("double-click");

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("http://backend/api/game/upgrades/double-click", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task BackendFailureIsPropagated()
    {
        var handler = new StateHandler(CreateState(), HttpStatusCode.InternalServerError);
        var model = CreateModel(handler);

        await Assert.ThrowsAsync<HttpRequestException>(() => model.OnGetAsync());
    }

    [Fact]
    public void GameStateDefaultsAreSafeForEmptyResponses()
    {
        var state = new GameState();

        Assert.Equal(0, state.Points);
        Assert.Equal(1, state.ClickValue);
        Assert.Empty(state.Upgrades);
    }

    private static IndexModel CreateModel(StateHandler handler)
    {
        var httpContext = new DefaultHttpContext();

        var model = new IndexModel(new TestHttpClientFactory(handler))
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };
        return model;
    }

    private static GameState CreateState(long points = 0, int clickValue = 1) => new()
    {
        Points = points,
        ClickValue = clickValue,
        Upgrades = new Dictionary<string, UpgradeData>
        {
            ["auto-clicker"] = new() { Id = "auto-clicker", PointsPerSecond = 1 }
        }
    };

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public TestHttpClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name) => new(_handler)
        {
            BaseAddress = new Uri("http://backend/")
        };
    }

    private sealed class StateHandler : HttpMessageHandler
    {
        private readonly GameState _state;
        private readonly HttpStatusCode _statusCode;

        public StateHandler(GameState state, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _state = state;
            _statusCode = statusCode;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            var response = new HttpResponseMessage(_statusCode)
            {
                RequestMessage = request
            };
            if (_statusCode == HttpStatusCode.OK)
            {
                response.Content = JsonContent.Create(_state, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }

            return Task.FromResult(response);
        }
    }
}
