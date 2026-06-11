using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Positions.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class PositionTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task PlacePosition_WithValidData_ReturnsCreated()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"pos_{suffix}", $"pos_{suffix}@example.com");
        var marketId = await CreateMarket($"Pos Market {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 500m);

        var response = await Client.PostAsJsonAsync("/api/positions", new
        {
            userId,
            marketId,
            outcomeId,
            amount = 100.0m
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetPositions_ReturnsList()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"poslist_{suffix}", $"poslist_{suffix}@example.com");
        var marketId = await CreateMarket($"Pos List Mkt {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 500m);

        await Client.PostAsJsonAsync("/api/positions", new
        {
            userId,
            marketId,
            outcomeId,
            amount = 50.0m
        });

        var response = await Client.GetAsync("/api/positions");
        response.EnsureSuccessStatusCode();

        var positions = await response.Content.ReadFromJsonAsync<List<PositionDto>>();
        Assert.NotNull(positions);
        Assert.Contains(positions, p => p.UserId == userId);
    }

    [Fact]
    public async Task GetPosition_WithExistingId_ReturnsPosition()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"posid_{suffix}", $"posid_{suffix}@example.com");
        var marketId = await CreateMarket($"Pos ID Mkt {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 500m);

        var postResponse = await Client.PostAsJsonAsync("/api/positions", new
        {
            userId,
            marketId,
            outcomeId,
            amount = 75.0m
        });
        var location = postResponse.Headers.Location!;
        var positionId = Guid.Parse(location.Segments[^1]);

        var response = await Client.GetAsync($"/api/positions/{positionId}");
        response.EnsureSuccessStatusCode();

        var position = await response.Content.ReadFromJsonAsync<PositionDto>();
        Assert.NotNull(position);
        Assert.Equal(positionId, position.Id);
        Assert.Equal(userId, position.UserId);
    }

    [Fact]
    public async Task GetPosition_WithNonExistentId_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/positions/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPositionsByMarket_ReturnsPositions()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"posbm_{suffix}", $"posbm_{suffix}@example.com");
        var marketId = await CreateMarket($"Pos BM Mkt {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 500m);

        await Client.PostAsJsonAsync("/api/positions", new
        {
            userId,
            marketId,
            outcomeId,
            amount = 60.0m
        });

        var response = await Client.GetAsync($"/api/positions/by-market/{marketId}?userId={userId}");
        response.EnsureSuccessStatusCode();

        var positions = await response.Content.ReadFromJsonAsync<List<PositionDto>>();
        Assert.NotNull(positions);
        Assert.NotEmpty(positions);
    }

    [Fact]
    public async Task GetPositionsWithPnL_ReturnsPositions()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"pospnl_{suffix}", $"pospnl_{suffix}@example.com");
        var marketId = await CreateMarket($"Pos PnL Mkt {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 500m);

        await Client.PostAsJsonAsync("/api/positions", new
        {
            userId,
            marketId,
            outcomeId,
            amount = 80.0m
        });

        TestAuthHandler.CurrentUserId = userId;

        var response = await Client.GetAsync("/api/positions/with-pnl");
        response.EnsureSuccessStatusCode();

        var positions = await response.Content.ReadFromJsonAsync<List<PositionWithPnLDto>>();
        Assert.NotNull(positions);
    }

    [Fact]
    public async Task ClosePosition_WithExistingId_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"poscl_{suffix}", $"poscl_{suffix}@example.com");
        var marketId = await CreateMarket($"Pos Close Mkt {suffix}", "Desc", userId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(userId, 500m);

        var postResponse = await Client.PostAsJsonAsync("/api/positions", new
        {
            userId,
            marketId,
            outcomeId,
            amount = 40.0m
        });
        var location = postResponse.Headers.Location!;
        var positionId = Guid.Parse(location.Segments[^1]);

        var response = await Client.PostAsync($"/api/positions/{positionId}/close?userId={userId}", null);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ClosePosition_ByDifferentUser_ReturnsError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var ownerId = await RegisterUser($"posco_{suffix}", $"posco_{suffix}@example.com");
        var marketId = await CreateMarket($"Pos CO Mkt {suffix}", "Desc", ownerId, ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);
        await CreditWallet(ownerId, 500m);

        var postResponse = await Client.PostAsJsonAsync("/api/positions", new
        {
            userId = ownerId,
            marketId,
            outcomeId,
            amount = 40.0m
        });
        var positionId = Guid.Parse(postResponse.Headers.Location!.Segments[^1]);
        var otherId = await RegisterUser($"posco2_{suffix}", $"posco2_{suffix}@example.com");

        var response = await Client.PostAsync($"/api/positions/{positionId}/close?userId={otherId}", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
