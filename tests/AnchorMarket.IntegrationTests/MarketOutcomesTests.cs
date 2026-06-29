using System.Net.Http.Json;
using AnchorMarket.Application.Features.Markets.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class MarketOutcomesTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task GetOutcomes_ReturnsMarketsOutcomes()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var creator = await RegisterUser($"out_{suffix}", $"out_{suffix}@x.com");
        var marketId = await CreateMarket($"BTC Up/Down {suffix}", "Up or down?", creator, ["UP", "DOWN"]);

        var response = await Client.GetAsync($"/api/markets/{marketId}/outcomes");
        response.EnsureSuccessStatusCode();

        var outcomes = await response.Content.ReadFromJsonAsync<List<OutcomeDto>>();
        Assert.NotNull(outcomes);
        Assert.Equal(2, outcomes.Count);
        Assert.Contains(outcomes, o => o.Title == "UP");
        Assert.Contains(outcomes, o => o.Title == "DOWN");
        Assert.All(outcomes, o => Assert.Equal(marketId, o.MarketId));
    }

    [Fact]
    public async Task GetOutcomes_UnknownMarket_ReturnsEmptyList()
    {
        var response = await Client.GetAsync($"/api/markets/{Guid.NewGuid()}/outcomes");
        response.EnsureSuccessStatusCode();

        var outcomes = await response.Content.ReadFromJsonAsync<List<OutcomeDto>>();
        Assert.NotNull(outcomes);
        Assert.Empty(outcomes);
    }
}
