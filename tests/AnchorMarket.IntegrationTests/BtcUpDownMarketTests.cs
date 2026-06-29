using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.ExternalFeeds.DTOs;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Application.Features.MarketResolutions.DTOs;
using AnchorMarket.Application.Features.Positions.DTOs;
using AnchorMarket.Application.Features.TradeFlow.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class BtcUpDownMarketTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    /// <summary>
    /// End-to-end "BTC Up or Down" market: create the binary market, attach a Binance feed, trade
    /// UP between two users, match the orders, then resolve UP as the winner and verify the winner
    /// read endpoint and settled position fair values.
    /// </summary>
    [Fact]
    public async Task BtcUpDown_CreateFeedTradeMatchResolve_Works()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var alice = await RegisterUser($"btc_alice_{suffix}", $"btc_alice_{suffix}@x.com");
        var bob = await RegisterUser($"btc_bob_{suffix}", $"btc_bob_{suffix}@x.com");

        // 1. Create the binary UP/DOWN market.
        var marketId = await CreateMarket(
            $"BTC Up or Down — next 5 minutes {suffix}",
            "Will BTCUSDT be higher 5 minutes from now?",
            alice, ["UP", "DOWN"]);

        // 2. Outcomes are discoverable via the API.
        var outcomesResponse = await Client.GetAsync($"/api/markets/{marketId}/outcomes");
        outcomesResponse.EnsureSuccessStatusCode();
        var outcomeList = await outcomesResponse.Content.ReadFromJsonAsync<List<OutcomeDto>>();
        Assert.NotNull(outcomeList);
        Assert.Equal(2, outcomeList.Count);
        var upId = outcomeList.Single(o => o.Title == "UP").Id;

        // 3. Attach a Binance BTC feed (admin).
        TestAuthHandler.IsAdmin = true;
        var feedResponse = await Client.PostAsJsonAsync("/api/feeds/register", new
        {
            marketId,
            adapterType = "BinanceCrypto",
            config = "{ \"Symbol\": \"BTCUSDT\" }",
            pollingIntervalMs = 1000,
            timeoutMs = 3000,
            resolutionGranularitySeconds = 5
        });
        TestAuthHandler.IsAdmin = false;
        Assert.Equal(HttpStatusCode.Created, feedResponse.StatusCode);

        var feeds = await Client.GetFromJsonAsync<List<FeedRegistrationDto>>($"/api/feeds/market/{marketId}");
        Assert.NotNull(feeds);
        Assert.Single(feeds);

        // 4. Fund both traders.
        await CreditWallet(alice, 1000m);
        await CreditWallet(bob, 1000m);

        // 5. Alice acquires UP shares, lists a sell; Bob buys; orders match.
        TestAuthHandler.CurrentUserId = alice;
        var pos = await Client.PostAsJsonAsync("/api/positions", new { userId = alice, marketId, outcomeId = upId, amount = 100.0m });
        Assert.Equal(HttpStatusCode.Created, pos.StatusCode);

        await PlaceLimitOrder(alice, marketId, upId, side: 1, price: 0.60m, quantity: 80.0m);
        await PlaceLimitOrder(bob, marketId, upId, side: 0, price: 0.65m, quantity: 80.0m);

        TestAuthHandler.IsAdmin = true;
        var match = await Client.PostAsync($"/api/OrderBooks/market/{marketId}/match?outcomeId={upId}", null);
        TestAuthHandler.IsAdmin = false;
        match.EnsureSuccessStatusCode();
        var matchResult = await match.Content.ReadFromJsonAsync<MatchingResultDto>();
        Assert.NotNull(matchResult);
        Assert.Equal(1, matchResult.TradesExecuted);

        // 6. The trade is captured in trade flow history.
        var flows = await Client.GetFromJsonAsync<List<TradeFlowDto>>($"/api/trades/flow/{marketId}");
        Assert.NotNull(flows);
        Assert.Single(flows);

        // Bob now holds UP shares.
        TestAuthHandler.CurrentUserId = bob;
        var bobPositions = await Client.GetFromJsonAsync<List<PositionDto>>($"/api/positions/by-market/{marketId}");
        Assert.NotNull(bobPositions);
        Assert.Equal(80m, Assert.Single(bobPositions, p => p.OutcomeId == upId).Shares);

        // 7. Resolve UP as the winner (admin).
        TestAuthHandler.IsAdmin = true;
        var resolve = await Client.PostAsJsonAsync($"/api/markets/{marketId}/resolve",
            new { winningOutcomeId = upId, resolutionSource = "Binance BTCUSDT close > open" });
        TestAuthHandler.IsAdmin = false;
        Assert.Equal(HttpStatusCode.NoContent, resolve.StatusCode);

        // 8. The winner is exposed and the market is resolved.
        var resolution = await Client.GetFromJsonAsync<MarketResolutionDto>($"/api/markets/{marketId}/resolution");
        Assert.NotNull(resolution);
        Assert.Equal(upId, resolution.WinningOutcomeId);

        var market = await Client.GetFromJsonAsync<MarketDto>($"/api/markets/{marketId}");
        Assert.NotNull(market);
        Assert.Equal(Domain.Enums.MarketStatus.Resolved, market.Status);

        // 9. Bob's winning position is settled to fair value 1.0.
        TestAuthHandler.CurrentUserId = bob;
        var bobPnl = await Client.GetFromJsonAsync<List<PositionWithPnLDto>>("/api/positions/with-pnl");
        Assert.NotNull(bobPnl);
        Assert.Equal(1.0m, Assert.Single(bobPnl, p => p.OutcomeId == upId).CurrentFairValue);
    }
}
