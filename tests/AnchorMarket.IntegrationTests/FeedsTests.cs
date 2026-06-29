using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.ExternalFeeds.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class FeedsTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    private async Task<Guid> CreateBtcMarket(string suffix)
    {
        var creator = await RegisterUser($"feed_{suffix}", $"feed_{suffix}@x.com");
        return await CreateMarket($"BTC Up/Down {suffix}", "Will BTC be higher in 5 minutes?", creator, ["UP", "DOWN"]);
    }

    private static object BinanceFeedBody(Guid marketId) => new
    {
        marketId,
        adapterType = "BinanceCrypto",
        config = "{ \"Symbol\": \"BTCUSDT\" }",
        pollingIntervalMs = 1000,
        timeoutMs = 3000,
        resolutionGranularitySeconds = 5
    };

    [Fact]
    public async Task RegisterFeed_AsAdmin_CreatesAndIsRetrievable()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var marketId = await CreateBtcMarket(suffix);

        TestAuthHandler.IsAdmin = true;
        var create = await Client.PostAsJsonAsync("/api/feeds/register", BinanceFeedBody(marketId));
        TestAuthHandler.IsAdmin = false;

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var id = Guid.Parse(create.Headers.Location!.Segments[^1]);

        var get = await Client.GetAsync($"/api/feeds/{id}");
        get.EnsureSuccessStatusCode();
        var feed = await get.Content.ReadFromJsonAsync<FeedRegistrationDto>();
        Assert.NotNull(feed);
        Assert.Equal(marketId, feed.MarketId);
        Assert.Equal("BinanceCrypto", feed.AdapterType);
        Assert.True(feed.IsActive);
    }

    [Fact]
    public async Task GetFeedsByMarket_ReturnsRegisteredFeed()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var marketId = await CreateBtcMarket(suffix);

        TestAuthHandler.IsAdmin = true;
        await Client.PostAsJsonAsync("/api/feeds/register", BinanceFeedBody(marketId));
        TestAuthHandler.IsAdmin = false;

        var get = await Client.GetAsync($"/api/feeds/market/{marketId}");
        get.EnsureSuccessStatusCode();
        var feeds = await get.Content.ReadFromJsonAsync<List<FeedRegistrationDto>>();
        Assert.NotNull(feeds);
        Assert.Single(feeds);
        Assert.Equal("BinanceCrypto", feeds[0].AdapterType);
    }

    [Fact]
    public async Task RegisterFeed_UnknownAdapter_ReturnsBadRequest()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var marketId = await CreateBtcMarket(suffix);

        TestAuthHandler.IsAdmin = true;
        var create = await Client.PostAsJsonAsync("/api/feeds/register", new
        {
            marketId,
            adapterType = "TotallyMadeUpAdapter",
            config = "{}",
            pollingIntervalMs = 1000,
            timeoutMs = 3000,
            resolutionGranularitySeconds = 5
        });
        TestAuthHandler.IsAdmin = false;

        Assert.Equal(HttpStatusCode.BadRequest, create.StatusCode);
    }

    [Fact]
    public async Task RegisterFeed_InvalidConfigJson_ReturnsBadRequest()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var marketId = await CreateBtcMarket(suffix);

        TestAuthHandler.IsAdmin = true;
        var create = await Client.PostAsJsonAsync("/api/feeds/register", new
        {
            marketId,
            adapterType = "BinanceCrypto",
            config = "not-valid-json",
            pollingIntervalMs = 1000,
            timeoutMs = 3000,
            resolutionGranularitySeconds = 5
        });
        TestAuthHandler.IsAdmin = false;

        Assert.Equal(HttpStatusCode.BadRequest, create.StatusCode);
    }

    [Fact]
    public async Task RegisterFeed_AsNonAdmin_ReturnsForbidden()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var marketId = await CreateBtcMarket(suffix);

        var create = await Client.PostAsJsonAsync("/api/feeds/register", BinanceFeedBody(marketId));

        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
    }
}
