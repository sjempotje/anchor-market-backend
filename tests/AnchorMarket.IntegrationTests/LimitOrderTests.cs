using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Orders.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class LimitOrderTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task PlaceBuyLimitOrder_WithSufficientBalance_CreatesOrder()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"lo_{suffix}", $"lo_{suffix}@example.com");
        var marketId = await CreateMarket($"LO Market {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 1000m);

        var orderId = await PlaceLimitOrder(userId, marketId, outcomeId, side: 0, price: 0.55m, quantity: 100.0m);
        Assert.NotEqual(Guid.Empty, orderId);
    }

    [Fact]
    public async Task PlaceBuyLimitOrder_WithInsufficientBalance_ReturnsError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"loib_{suffix}", $"loib_{suffix}@example.com");
        var marketId = await CreateMarket($"LO Insuff {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);

        var response = await Client.PostAsJsonAsync("/api/limitorders", new
        {
            userId,
            marketId,
            outcomeId,
            side = 0,
            price = 0.55m,
            quantity = 100.0m,
            expiresAt = (DateTimeOffset?)DateTimeOffset.UtcNow.AddDays(7)
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CancelLimitOrder_WithExistingId_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"loc_{suffix}", $"loc_{suffix}@example.com");
        var marketId = await CreateMarket($"LO Cancel {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 1000m);

        var orderId = await PlaceLimitOrder(userId, marketId, outcomeId);
        TestAuthHandler.CurrentUserId = userId;
        var response = await Client.DeleteAsync($"/api/limitorders/{orderId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetLimitOrder_WithExistingId_ReturnsOrder()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"log_{suffix}", $"log_{suffix}@example.com");
        var marketId = await CreateMarket($"LO Get {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 1000m);

        var orderId = await PlaceLimitOrder(userId, marketId, outcomeId);
        TestAuthHandler.CurrentUserId = userId;

        var response = await Client.GetAsync($"/api/limitorders/{orderId}");
        response.EnsureSuccessStatusCode();

        var order = await response.Content.ReadFromJsonAsync<LimitOrderDetailDto>();
        Assert.NotNull(order);
        Assert.Equal(orderId, order.Id);
    }

    [Fact]
    public async Task GetLimitOrder_AsDifferentUser_ReturnsForbidden()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var ownerId = await RegisterUser($"lodf_{suffix}", $"lodf_{suffix}@example.com");
        var marketId = await CreateMarket($"LO Diff {suffix}", "Desc", ownerId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(ownerId, 1000m);

        var orderId = await PlaceLimitOrder(ownerId, marketId, outcomeId);

        var otherId = await RegisterUser($"lodf2_{suffix}", $"lodf2_{suffix}@example.com");
        TestAuthHandler.CurrentUserId = otherId;

        var response = await Client.GetAsync($"/api/limitorders/{orderId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetLimitOrdersByMarket_ReturnsOrders()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"lom_{suffix}", $"lom_{suffix}@example.com");
        var marketId = await CreateMarket($"LO Mkt {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 1000m);

        await PlaceLimitOrder(userId, marketId, outcomeId);

        TestAuthHandler.CurrentUserId = userId;
        var response = await Client.GetAsync($"/api/limitorders/market/{marketId}?outcomeId={outcomeId}");
        response.EnsureSuccessStatusCode();

        var orders = await response.Content.ReadFromJsonAsync<List<LimitOrderDto>>();
        Assert.NotNull(orders);
        Assert.NotEmpty(orders);
    }

    [Fact]
    public async Task PlaceSellLimitOrder_WithExistingPosition_CreatesOrder()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"los_{suffix}", $"los_{suffix}@example.com");
        var marketId = await CreateMarket($"LO Sell {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 1000m);

        var positionResponse = await Client.PostAsJsonAsync("/api/positions", new
        {
            userId,
            marketId,
            outcomeId,
            amount = 200.0
        });
        positionResponse.EnsureSuccessStatusCode();

        TestAuthHandler.CurrentUserId = userId;
        var orderId = await PlaceLimitOrder(userId, marketId, outcomeId, side: 1, price: 0.45m, quantity: 100.0m);
        Assert.NotEqual(Guid.Empty, orderId);
    }

    [Fact]
    public async Task PlaceSellLimitOrder_WithoutPosition_ReturnsError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"losnp_{suffix}", $"losnp_{suffix}@example.com");
        var marketId = await CreateMarket($"LO SellNP {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 1000m);

        var response = await Client.PostAsJsonAsync("/api/limitorders", new
        {
            userId,
            marketId,
            outcomeId,
            side = 1,
            price = 0.45m,
            quantity = 100.0m,
            expiresAt = (DateTimeOffset?)DateTimeOffset.UtcNow.AddDays(7)
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlaceLimitOrder_WithPriceTooLow_ReturnsError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"lopr_{suffix}", $"lopr_{suffix}@example.com");
        var marketId = await CreateMarket($"LO Pric {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 1000m);

        var response = await Client.PostAsJsonAsync("/api/limitorders", new
        {
            userId,
            marketId,
            outcomeId,
            side = 0,
            price = 0.001m,
            quantity = 100.0m,
            expiresAt = (DateTimeOffset?)DateTimeOffset.UtcNow.AddDays(7)
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlaceLimitOrder_WithPriceTooHigh_ReturnsError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"loph_{suffix}", $"loph_{suffix}@example.com");
        var marketId = await CreateMarket($"LO Phigh {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 1000m);

        var response = await Client.PostAsJsonAsync("/api/limitorders", new
        {
            userId,
            marketId,
            outcomeId,
            side = 0,
            price = 1.5m,
            quantity = 100.0m,
            expiresAt = (DateTimeOffset?)DateTimeOffset.UtcNow.AddDays(7)
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlaceLimitOrder_WithZeroQuantity_ReturnsError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"lozq_{suffix}", $"lozq_{suffix}@example.com");
        var marketId = await CreateMarket($"LO ZQ {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 1000m);

        var response = await Client.PostAsJsonAsync("/api/limitorders", new
        {
            userId,
            marketId,
            outcomeId,
            side = 0,
            price = 0.55m,
            quantity = 0.0m,
            expiresAt = (DateTimeOffset?)DateTimeOffset.UtcNow.AddDays(7)
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
