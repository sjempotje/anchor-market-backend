using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Application.Features.Orders.DTOs;
using AnchorMarket.Application.Features.Positions.DTOs;
using AnchorMarket.Application.Features.Wallets.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

public class MatchingScenarios(CustomWebApplicationFactory factory) : TestBase(factory)
{
    /// <summary>
    /// Full flow: place position, place limit orders, match them, resolve the market, verify PnL, close positions and verify payouts.
    /// </summary>
    [Fact]
    public async Task Btc80k_FullFlow_MatchingResolutionAndPayout()
    {
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var alice = await RegisterUser($"alice_{suffix}", $"alice_{suffix}@x.com");
        var bob = await RegisterUser($"bob_{suffix}", $"bob_{suffix}@x.com");
        var charlie = await RegisterUser($"charlie_{suffix}", $"charlie_{suffix}@x.com");

        var groupId = await CreateGroup($"BTC Group {suffix}", "BTC prediction market", alice);
        await AddGroupMembership(alice, groupId);
        await AddGroupMembership(bob, groupId);
        await AddGroupMembership(charlie, groupId);

        var marketId = await CreateGroupMarket(groupId, alice,
            "Will Bitcoin reach $80K by Dec 2026?", "BTC prediction market",
            ["Yes", "No"]);
        var outcomes = await GetOutcomes(marketId);
        var yesId = outcomes["Yes"];
        var noId = outcomes["No"];

        await CreditWallet(alice, 1000m);
        await CreditWallet(bob, 1000m);
        await CreditWallet(charlie, 1000m);

        var posResponse = await Client.PostAsJsonAsync("/api/positions", new
        {
            userId = alice,
            marketId,
            outcomeId = yesId,
            amount = 100.0m
        });
        Assert.Equal(HttpStatusCode.Created, posResponse.StatusCode);
        var alicePositionId = Guid.Parse(posResponse.Headers.Location!.Segments[^1]);

        var sellOrderId = await PlaceLimitOrder(alice, marketId, yesId, side: 1, price: 0.60m, quantity: 80.0m);

        var buyOrderId = await PlaceLimitOrder(bob, marketId, yesId, side: 0, price: 0.65m, quantity: 80.0m);

        var matchResponse = await Client.PostAsync($"/api/OrderBooks/market/{marketId}/match?outcomeId={yesId}", null);
        matchResponse.EnsureSuccessStatusCode();
        var matchResult = await matchResponse.Content.ReadFromJsonAsync<MatchingResultDto>();
        Assert.NotNull(matchResult);
        Assert.Equal(1, matchResult.TradesExecuted);
        Assert.Equal(80.0m * 0.625m, matchResult.TotalVolume);

        TestAuthHandler.CurrentUserId = bob;
        var bobBuyResponse = await Client.GetAsync($"/api/limitorders/{buyOrderId}");
        bobBuyResponse.EnsureSuccessStatusCode();
        var bobOrder = await bobBuyResponse.Content.ReadFromJsonAsync<LimitOrderDetailDto>();
        Assert.NotNull(bobOrder);
        Assert.Equal(Domain.Enums.OrderStatus.Filled, bobOrder.Status);

        var alicePosResponse = await Client.GetAsync($"/api/positions/by-market/{marketId}?userId={alice}");
        alicePosResponse.EnsureSuccessStatusCode();
        var alicePositions = await alicePosResponse.Content.ReadFromJsonAsync<List<PositionDto>>();
        Assert.NotNull(alicePositions);
        var aliceYesPos = Assert.Single(alicePositions, p => p.OutcomeId == yesId);
        // Alice started with 200 shares (100 / 0.5), sold 80 → 120 remaining
        Assert.Equal(120m, aliceYesPos.Shares);

        var bobPosResponse = await Client.GetAsync($"/api/positions/by-market/{marketId}?userId={bob}");
        bobPosResponse.EnsureSuccessStatusCode();
        var bobPositions = await bobPosResponse.Content.ReadFromJsonAsync<List<PositionDto>>();
        Assert.NotNull(bobPositions);
        var bobYesPos = Assert.Single(bobPositions, p => p.OutcomeId == yesId);
        // Bob bought 80 shares at 0.625 mid-price
        Assert.Equal(80m, bobYesPos.Shares);

        var resolveResponse = await Client.PostAsJsonAsync($"/api/group-markets/{marketId}/resolve", new
        {
            marketId,
            winningOutcomeId = yesId,
            resolverId = charlie
        });
        Assert.Equal(HttpStatusCode.NoContent, resolveResponse.StatusCode);

        var marketResponse = await Client.GetAsync($"/api/markets/{marketId}");
        marketResponse.EnsureSuccessStatusCode();
        var resolvedMarket = await marketResponse.Content.ReadFromJsonAsync<MarketDto>();
        Assert.NotNull(resolvedMarket);
        Assert.Equal(Domain.Enums.MarketStatus.Resolved, resolvedMarket.Status);

        TestAuthHandler.CurrentUserId = alice;
        var alicePnlResponse = await Client.GetAsync("/api/positions/with-pnl");
        alicePnlResponse.EnsureSuccessStatusCode();
        var alicePnl = await alicePnlResponse.Content.ReadFromJsonAsync<List<PositionWithPnLDto>>();
        Assert.NotNull(alicePnl);
        var alicePnlPos = Assert.Single(alicePnl, p => p.OutcomeId == yesId);
        Assert.Equal(1.0m, alicePnlPos.CurrentFairValue);
        Assert.True(alicePnlPos.UnrealizedPnL > 0);

        TestAuthHandler.CurrentUserId = bob;
        var bobPnlResponse = await Client.GetAsync("/api/positions/with-pnl");
        bobPnlResponse.EnsureSuccessStatusCode();
        var bobPnl = await bobPnlResponse.Content.ReadFromJsonAsync<List<PositionWithPnLDto>>();
        Assert.NotNull(bobPnl);
        var bobPnlPos = Assert.Single(bobPnl, p => p.OutcomeId == yesId);
        Assert.Equal(1.0m, bobPnlPos.CurrentFairValue);

        var aliceWalletId = await GetWalletId(alice);
        var bobWalletId = await GetWalletId(bob);

        var aliceWalletBefore = await Client.GetFromJsonAsync<WalletDto>($"/api/wallets/{aliceWalletId}");
        Assert.NotNull(aliceWalletBefore);
        var bobWalletBefore = await Client.GetFromJsonAsync<WalletDto>($"/api/wallets/{bobWalletId}");
        Assert.NotNull(bobWalletBefore);

        var closeAliceResponse = await Client.PostAsync(
            $"/api/positions/{alicePositionId}/close?userId={alice}", null);
        Assert.Equal(HttpStatusCode.NoContent, closeAliceResponse.StatusCode);

        var bobPosId = bobPositions.First(p => p.OutcomeId == yesId).Id;
        var closeBobResponse = await Client.PostAsync(
            $"/api/positions/{bobPosId}/close?userId={bob}", null);
        Assert.Equal(HttpStatusCode.NoContent, closeBobResponse.StatusCode);

        // Verify payouts: both won, so both should have more money after close
        var aliceWalletAfter = await Client.GetFromJsonAsync<WalletDto>($"/api/wallets/{aliceWalletId}");
        Assert.NotNull(aliceWalletAfter);
        Assert.True(aliceWalletAfter.Balance > aliceWalletBefore.Balance,
            $"Alice balance should increase: {aliceWalletAfter.Balance} > {aliceWalletBefore.Balance}");

        var bobWalletAfter = await Client.GetFromJsonAsync<WalletDto>($"/api/wallets/{bobWalletId}");
        Assert.NotNull(bobWalletAfter);
        Assert.True(bobWalletAfter.Balance > bobWalletBefore.Balance,
            $"Bob balance should increase: {bobWalletAfter.Balance} > {bobWalletBefore.Balance}");
    }
}

/// <summary>Local DTO for MatchingResult since the endpoint returns domain entities directly.</summary>
public record MatchingResultDto(
    int TradesExecuted,
    decimal TotalVolume);
