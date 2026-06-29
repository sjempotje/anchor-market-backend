using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Orders.DTOs;
using AnchorMarket.Application.Features.Positions.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class OrderBookMarketTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    private async Task<bool> TryPlaceOrder(Guid userId, Guid marketId, Guid outcomeId, int side, decimal price, decimal quantity)
    {
        TestAuthHandler.CurrentUserId = userId;
        var response = await Client.PostAsJsonAsync("/api/limitorders", new
        {
            marketId,
            outcomeId,
            side,
            price,
            quantity,
            expiresAt = (DateTimeOffset?)DateTimeOffset.UtcNow.AddDays(7)
        });
        return response.StatusCode == HttpStatusCode.Created;
    }

    private async Task<OrderBookDto> GetOrderBook(Guid marketId, Guid outcomeId)
        => (await Client.GetFromJsonAsync<OrderBookDto>($"/api/OrderBooks/market/{marketId}/outcome/{outcomeId}"))!;

    private async Task<MarketPriceDto> GetMarketPrice(Guid marketId, Guid outcomeId)
        => (await Client.GetFromJsonAsync<MarketPriceDto>($"/api/OrderBooks/market/{marketId}/outcome/{outcomeId}/price"))!;

    private async Task<decimal> GetOutcomeShares(Guid userId, Guid marketId, Guid outcomeId)
    {
        TestAuthHandler.CurrentUserId = userId;
        var positions = await Client.GetFromJsonAsync<List<PositionDto>>($"/api/positions/by-market/{marketId}");
        return positions?.Where(p => p.OutcomeId == outcomeId).Sum(p => p.Shares) ?? 0m;
    }

    private static void AssertBookConsistent(OrderBookDto book)
    {
        // Bids strictly descending, asks strictly ascending (grouped by price level).
        for (var i = 1; i < book.Bids.Count; i++)
            Assert.True(book.Bids[i - 1].Price > book.Bids[i].Price, "bids must be sorted descending");
        for (var i = 1; i < book.Asks.Count; i++)
            Assert.True(book.Asks[i - 1].Price < book.Asks[i].Price, "asks must be sorted ascending");

        // Every resting level has positive remaining quantity and at least one order.
        foreach (var level in book.Bids.Concat(book.Asks))
        {
            Assert.True(level.TotalQuantity > 0, "level quantity must be positive");
            Assert.True(level.OrderCount >= 1, "level must aggregate at least one order");
        }

        // Best bid/ask reflect the top of each side.
        Assert.Equal(book.Bids.Count > 0 ? book.Bids[0].Price : (decimal?)null, book.BestBid);
        Assert.Equal(book.Asks.Count > 0 ? book.Asks[0].Price : (decimal?)null, book.BestAsk);
    }

    [Fact]
    public async Task OrderBook_DeterministicLevels_AggregateAndSortCorrectly()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var alice = await RegisterUser($"obd_a_{suffix}", $"obd_a_{suffix}@x.com");
        var bob = await RegisterUser($"obd_b_{suffix}", $"obd_b_{suffix}@x.com");
        await CreditWallet(alice, 100_000m);
        await CreditWallet(bob, 100_000m);

        var marketId = await CreateMarket($"BTC Up/Down {suffix}", "Up or down?", alice, ["UP", "DOWN"]);
        var upId = (await GetOutcomes(marketId))["UP"];

        // Two non-crossing bids at the same level + a higher bid; one ask well above.
        Assert.True(await TryPlaceOrder(alice, marketId, upId, side: 0, price: 0.40m, quantity: 100m));
        Assert.True(await TryPlaceOrder(bob, marketId, upId, side: 0, price: 0.40m, quantity: 50m));
        Assert.True(await TryPlaceOrder(alice, marketId, upId, side: 0, price: 0.45m, quantity: 30m));
        Assert.True(await TryPlaceOrder(bob, marketId, upId, side: 0, price: 0.30m, quantity: 10m));

        var book = await GetOrderBook(marketId, upId);
        AssertBookConsistent(book);

        Assert.Equal(0.45m, book.BestBid);
        Assert.Null(book.BestAsk);
        // Three distinct bid levels, top first.
        Assert.Equal([0.45m, 0.40m, 0.30m], book.Bids.Select(b => b.Price).ToArray());
        // The 0.40 level aggregates both orders: 100 + 50 = 150 across 2 orders.
        var level40 = book.Bids.Single(b => b.Price == 0.40m);
        Assert.Equal(150m, level40.TotalQuantity);
        Assert.Equal(2, level40.OrderCount);
    }

    [Fact]
    public async Task OrderBook_RandomOrders_StayConsistent_PricesWork_AndSharesAreConserved()
    {
        var rng = new Random(20260625);
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Five funded traders, each seeded with a position so they can both buy and sell.
        var users = new List<Guid>();
        for (var i = 0; i < 5; i++)
        {
            var u = await RegisterUser($"obr_{i}_{suffix}", $"obr_{i}_{suffix}@x.com");
            await CreditWallet(u, 1_000_000m);
            users.Add(u);
        }

        var marketId = await CreateMarket($"BTC Up/Down {suffix}", "Up or down?", users[0], ["UP", "DOWN"]);
        var upId = (await GetOutcomes(marketId))["UP"];

        foreach (var u in users)
        {
            TestAuthHandler.CurrentUserId = u;
            var pos = await Client.PostAsJsonAsync("/api/positions", new { userId = u, marketId, outcomeId = upId, amount = 5000m });
            Assert.Equal(HttpStatusCode.Created, pos.StatusCode);
        }

        var initialShares = 0m;
        foreach (var u in users)
            initialShares += await GetOutcomeShares(u, marketId, upId);
        Assert.True(initialShares > 0);

        // Fire a burst of random buy/sell orders at varying prices and sizes.
        var placed = 0;
        for (var n = 0; n < 80; n++)
        {
            var user = users[rng.Next(users.Count)];
            var side = rng.Next(2); // 0 = buy, 1 = sell
            var price = Math.Round(rng.Next(5, 96) / 100m, 2); // 0.05 .. 0.95
            var quantity = rng.Next(1, 16);
            if (await TryPlaceOrder(user, marketId, upId, side, price, quantity))
                placed++;
        }
        Assert.True(placed > 0, "expected at least some random orders to rest or match");

        // Book is well-formed before matching.
        AssertBookConsistent(await GetOrderBook(marketId, upId));

        // Run the matching engine.
        TestAuthHandler.IsAdmin = true;
        var match = await Client.PostAsync($"/api/OrderBooks/market/{marketId}/match?outcomeId={upId}", null);
        TestAuthHandler.IsAdmin = false;
        match.EnsureSuccessStatusCode();

        // Book is well-formed and uncrossed after matching.
        var book = await GetOrderBook(marketId, upId);
        AssertBookConsistent(book);
        if (book.BestBid.HasValue && book.BestAsk.HasValue)
            Assert.True(book.BestBid.Value < book.BestAsk.Value, "matched book must not be crossed");

        // Price endpoint is sane and matches the mid when both sides are present.
        var marketPrice = await GetMarketPrice(marketId, upId);
        Assert.InRange(marketPrice.CurrentPrice, 0m, 1m);
        if (book.BestBid.HasValue && book.BestAsk.HasValue)
            Assert.Equal((book.BestBid.Value + book.BestAsk.Value) / 2, marketPrice.CurrentPrice);

        // Trading only transfers shares between users — the outcome's total is conserved.
        var finalShares = 0m;
        foreach (var u in users)
            finalShares += await GetOutcomeShares(u, marketId, upId);
        Assert.Equal(initialShares, finalShares);
    }
}
