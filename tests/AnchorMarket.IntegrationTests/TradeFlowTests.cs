using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.TradeFlow.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class TradeFlowTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task Matching_RecordsTradeFlowSnapshots()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var alice = await RegisterUser($"tf_alice_{suffix}", $"tf_alice_{suffix}@x.com");
        var bob = await RegisterUser($"tf_bob_{suffix}", $"tf_bob_{suffix}@x.com");

        var marketId = await CreateMarket($"BTC Up/Down {suffix}", "Up or down?", alice, ["UP", "DOWN"]);
        var outcomes = await GetOutcomes(marketId);
        var upId = outcomes["UP"];

        await CreditWallet(alice, 1000m);
        await CreditWallet(bob, 1000m);

        // Alice acquires shares to sell.
        TestAuthHandler.CurrentUserId = alice;
        var pos = await Client.PostAsJsonAsync("/api/positions", new { userId = alice, marketId, outcomeId = upId, amount = 100.0m });
        Assert.Equal(HttpStatusCode.Created, pos.StatusCode);

        await PlaceLimitOrder(alice, marketId, upId, side: 1, price: 0.60m, quantity: 80.0m);
        await PlaceLimitOrder(bob, marketId, upId, side: 0, price: 0.65m, quantity: 80.0m);

        TestAuthHandler.IsAdmin = true;
        var match = await Client.PostAsync($"/api/OrderBooks/market/{marketId}/match?outcomeId={upId}", null);
        TestAuthHandler.IsAdmin = false;
        match.EnsureSuccessStatusCode();

        var flowResponse = await Client.GetAsync($"/api/trades/flow/{marketId}");
        flowResponse.EnsureSuccessStatusCode();
        var flows = await flowResponse.Content.ReadFromJsonAsync<List<TradeFlowDto>>();

        Assert.NotNull(flows);
        var flow = Assert.Single(flows);
        Assert.Equal(upId, flow.OutcomeId);
        Assert.Equal(80.0m, flow.Shares);
        Assert.Equal(0.625m, flow.ExecutedPrice); // mid of 0.60 / 0.65
        Assert.True(flow.BidDepthAtTrade >= 80m);
        Assert.True(flow.AskDepthAtTrade >= 80m);
    }
}
