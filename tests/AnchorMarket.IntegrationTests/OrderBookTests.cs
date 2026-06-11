using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Orders.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

internal record MatchingResult(int TradesExecuted, decimal TotalVolume, IReadOnlyList<TradeExecution> ExecutedTrades);

internal record TradeExecution(
    Guid LimitOrderId,
    Guid BuyerOrderId,
    Guid SellerOrderId,
    Guid InitiatorUserId,
    decimal FilledQuantity,
    decimal ExecutedPrice);

[Collection("IntegrationTests")]
public class OrderBookTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task GetOrderBook_ReturnsOrderBook()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"ob_{suffix}", $"ob_{suffix}@example.com");
        var marketId = await CreateMarket($"OB Market {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);

        var response = await Client.GetAsync($"/api/orderbooks/market/{marketId}/outcome/{outcomeId}");
        response.EnsureSuccessStatusCode();

        var orderBook = await response.Content.ReadFromJsonAsync<OrderBookDto>();
        Assert.NotNull(orderBook);
        Assert.Equal(marketId, orderBook.MarketId);
        Assert.Equal(outcomeId, orderBook.OutcomeId);
    }

    [Fact]
    public async Task GetOrderBook_WithNonExistentMarket_ReturnsError()
    {
        var response = await Client.GetAsync($"/api/orderbooks/market/{Guid.NewGuid()}/outcome/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMarketPrice_ReturnsPrice()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"mp_{suffix}", $"mp_{suffix}@example.com");
        var marketId = await CreateMarket($"MP Market {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);

        var response = await Client.GetAsync($"/api/orderbooks/market/{marketId}/outcome/{outcomeId}/price");
        response.EnsureSuccessStatusCode();

        var price = await response.Content.ReadFromJsonAsync<MarketPriceDto>();
        Assert.NotNull(price);
        Assert.Equal(marketId, price.MarketId);
        Assert.Equal(outcomeId, price.OutcomeId);
    }

    [Fact]
    public async Task GetMarketPrice_WithNonExistentMarket_ReturnsError()
    {
        var response = await Client.GetAsync($"/api/orderbooks/market/{Guid.NewGuid()}/outcome/{Guid.NewGuid()}/price");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MatchOrders_ReturnsMatchingResult()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"match_{suffix}", $"match_{suffix}@example.com");
        var marketId = await CreateMarket($"Match Market {suffix}", "Desc", userId, ["Yes", "No"]);

        var response = await Client.PostAsync($"/api/orderbooks/market/{marketId}/match", null);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MatchingResult>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task MatchOrders_WithNonExistentMarket_ReturnsEmptyResult()
    {
        var response = await Client.PostAsync($"/api/orderbooks/market/{Guid.NewGuid()}/match", null);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MatchingResult>();
        Assert.NotNull(result);
        Assert.Equal(0, result.TradesExecuted);
    }
}
