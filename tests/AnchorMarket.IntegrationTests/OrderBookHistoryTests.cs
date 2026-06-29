using System.Net.Http.Json;
using AnchorMarket.Application.Features.OrderBookHistory.DTOs;
using AnchorMarket.Domain.Entities;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class OrderBookHistoryTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task GetOrderBookHistory_ReturnsSeededSnapshots()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var creator = await RegisterUser($"obh_{suffix}", $"obh_{suffix}@x.com");
        var marketId = await CreateMarket($"BTC Up/Down {suffix}", "Up or down?", creator, ["UP", "DOWN"]);
        var outcomes = await GetOutcomes(marketId);
        var upId = outcomes["UP"];

        // The snapshot writer is a background service (disabled in tests), so seed a snapshot directly.
        await using (var db = Factory.CreateDbContext())
        {
            db.OrderBookSnapshots.Add(OrderBookSnapshot.Create(
                upId, DateTimeOffset.UtcNow,
                bids: "[{\"Price\":0.55,\"Quantity\":100}]",
                asks: "[{\"Price\":0.60,\"Quantity\":80}]",
                bestBid: 0.55m, bestAsk: 0.60m));
            await db.SaveChangesAsync();
        }

        var response = await Client.GetAsync($"/api/orderbook/history/{upId}");
        response.EnsureSuccessStatusCode();
        var snapshots = await response.Content.ReadFromJsonAsync<List<OrderBookSnapshotDto>>();

        Assert.NotNull(snapshots);
        var snapshot = Assert.Single(snapshots);
        Assert.Equal(upId, snapshot.OutcomeId);
        Assert.Equal(0.55m, snapshot.BestBid);
        Assert.Equal(0.60m, snapshot.BestAsk);
        Assert.Equal(0.05m, snapshot.Spread);
    }

    [Fact]
    public async Task GetOrderBookHistory_NoSnapshots_ReturnsEmpty()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await RegisterUser($"obhe_{suffix}", $"obhe_{suffix}@x.com");

        var response = await Client.GetAsync($"/api/orderbook/history/{Guid.NewGuid()}");
        response.EnsureSuccessStatusCode();
        var snapshots = await response.Content.ReadFromJsonAsync<List<OrderBookSnapshotDto>>();

        Assert.NotNull(snapshots);
        Assert.Empty(snapshots);
    }
}
