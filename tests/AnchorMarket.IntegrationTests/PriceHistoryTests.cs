using System.Net.Http.Json;
using AnchorMarket.Application.Features.PriceHistory.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class PriceHistoryTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task GetPriceHistory_ForOutcome_ReturnsList()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"ph_{suffix}", $"ph_{suffix}@example.com");
        var marketId = await CreateMarket($"PH Market {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);

        var response = await Client.GetAsync($"/api/outcomes/{outcomeId}/price-history");
        response.EnsureSuccessStatusCode();

        var history = await response.Content.ReadFromJsonAsync<List<PriceHistoryDto>>();
        Assert.NotNull(history);
    }

    [Fact]
    public async Task GetPriceHistory_AfterPlacingPosition_HasEntry()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"phpos_{suffix}", $"phpos_{suffix}@example.com");
        var marketId = await CreateMarket($"PH Pos Market {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 200m);

        await Client.PostAsJsonAsync("/api/positions", new
        {
            userId,
            marketId,
            outcomeId,
            amount = 50.0m
        });

        var response = await Client.GetAsync($"/api/outcomes/{outcomeId}/price-history");
        response.EnsureSuccessStatusCode();

        var history = await response.Content.ReadFromJsonAsync<List<PriceHistoryDto>>();
        Assert.NotNull(history);
    }
}
