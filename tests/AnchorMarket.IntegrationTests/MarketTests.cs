using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Markets.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

public class MarketTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task CreateMarket_ReturnsCreated()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"mkt_{suffix}", $"mkt_{suffix}@example.com");
        var marketId = await CreateMarket($"Market {suffix}", "Test market", userId, ["Yes", "No"]);
        Assert.NotEqual(Guid.Empty, marketId);
    }

    [Fact]
    public async Task GetMarkets_ReturnsList()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"mktlist_{suffix}", $"mktlist_{suffix}@example.com");
        await CreateMarket($"Mkt A {suffix}", "Desc", userId, ["Yes", "No"]);

        var response = await Client.GetAsync("/api/markets");
        response.EnsureSuccessStatusCode();

        var markets = await response.Content.ReadFromJsonAsync<List<MarketDto>>();
        Assert.NotNull(markets);
        Assert.Contains(markets, m => m.Title.Contains(suffix));
    }

    [Fact]
    public async Task GetMarket_WithExistingId_ReturnsMarket()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"mktid_{suffix}", $"mktid_{suffix}@example.com");
        var marketId = await CreateMarket($"Specific Mkt {suffix}", "Specific desc", userId, ["Yes", "No"]);

        var response = await Client.GetAsync($"/api/markets/{marketId}");
        response.EnsureSuccessStatusCode();

        var market = await response.Content.ReadFromJsonAsync<MarketDto>();
        Assert.NotNull(market);
        Assert.Equal(marketId, market.Id);
        Assert.Equal(userId, market.CreatorId);
    }

    [Fact]
    public async Task UpdateMarket_WithValidData_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"mktupd_{suffix}", $"mktupd_{suffix}@example.com");
        var marketId = await CreateMarket($"Original {suffix}", "Original desc", userId, ["Yes", "No"]);

        var newDeadline = DateTimeOffset.UtcNow.AddDays(60);
        var response = await Client.PutAsJsonAsync($"/api/markets/{marketId}", new
        {
            marketId,
            title = $"Updated {suffix}",
            description = "Updated description",
            resolutionDeadline = newDeadline
        });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync($"/api/markets/{marketId}");
        var market = await getResponse.Content.ReadFromJsonAsync<MarketDto>();
        Assert.Equal($"Updated {suffix}", market!.Title);
    }

    [Fact]
    public async Task DeleteMarket_WithExistingId_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"mktdel_{suffix}", $"mktdel_{suffix}@example.com");
        var marketId = await CreateMarket($"Delete Mkt {suffix}", "Desc", userId, ["Yes", "No"]);

        var response = await Client.DeleteAsync($"/api/markets/{marketId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync($"/api/markets/{marketId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateGroupMarket_ReturnsCreated()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"gm_{suffix}", $"gm_{suffix}@example.com");
        var groupId = await CreateGroup($"GM Group {suffix}", null, userId);
        await AddGroupMembership(userId, groupId);
        var marketId = await CreateGroupMarket(groupId, userId, $"GM Market {suffix}", "Group market description", ["Yes", "No"]);
        Assert.NotEqual(Guid.Empty, marketId);
    }

    [Fact]
    public async Task GetGroupMarkets_ReturnsList()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"gmlist_{suffix}", $"gmlist_{suffix}@example.com");
        var groupId = await CreateGroup($"GM List Group {suffix}", null, userId);
        await AddGroupMembership(userId, groupId);
        await CreateGroupMarket(groupId, userId, $"GM List Mkt {suffix}", "Desc", ["Yes", "No"]);

        var response = await Client.GetAsync($"/api/group-markets?groupId={groupId}&requestingUserId={userId}");
        response.EnsureSuccessStatusCode();

        var markets = await response.Content.ReadFromJsonAsync<List<MarketDto>>();
        Assert.NotNull(markets);
        Assert.Contains(markets, m => m.Title.Contains(suffix));
    }

    [Fact]
    public async Task GetGroupMarkets_NonMember_ReturnsEmpty()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var ownerId = await RegisterUser($"gmem_{suffix}", $"gmem_{suffix}@example.com");
        var groupId = await CreateGroup($"GM Empty {suffix}", null, ownerId);
        await AddGroupMembership(ownerId, groupId);
        await CreateGroupMarket(groupId, ownerId, $"GM Empty Mkt {suffix}", "Desc", ["Yes", "No"]);

        var nonMemberId = await RegisterUser($"gmem2_{suffix}", $"gmem2_{suffix}@example.com");

        var response = await Client.GetAsync($"/api/group-markets?groupId={groupId}&requestingUserId={nonMemberId}");
        response.EnsureSuccessStatusCode();

        var markets = await response.Content.ReadFromJsonAsync<List<MarketDto>>();
        Assert.NotNull(markets);
        Assert.Empty(markets);
    }

    [Fact]
    public async Task GetGroupMarketById_WithExistingId_ReturnsMarket()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"gmbi_{suffix}", $"gmbi_{suffix}@example.com");
        var groupId = await CreateGroup($"GM ById {suffix}", null, userId);
        await AddGroupMembership(userId, groupId);
        var marketId = await CreateGroupMarket(groupId, userId, $"GM ById Mkt {suffix}", "Desc", ["Yes", "No"]);

        var response = await Client.GetAsync($"/api/group-markets/{marketId}");
        response.EnsureSuccessStatusCode();

        var market = await response.Content.ReadFromJsonAsync<MarketDto>();
        Assert.NotNull(market);
        Assert.Equal(marketId, market.Id);
        Assert.Equal(userId, market.CreatorId);
    }

    [Fact]
    public async Task GetGroupMarketById_WithNonExistentId_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/group-markets/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ResolveGroupMarket_WithValidData_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var creatorId = await RegisterUser($"gmres_{suffix}", $"gmres_{suffix}@example.com");
        var groupId = await CreateGroup($"GM Resolve {suffix}", null, creatorId);
        await AddGroupMembership(creatorId, groupId);
        var marketId = await CreateGroupMarket(groupId, creatorId, $"GM Resolve Mkt {suffix}", "Desc", ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);

        var resolverId = await RegisterUser($"gmres2_{suffix}", $"gmres2_{suffix}@example.com");
        await AddGroupMembership(resolverId, groupId);

        var response = await Client.PostAsJsonAsync($"/api/group-markets/{marketId}/resolve", new
        {
            marketId,
            winningOutcomeId = outcomeId,
            resolverId
        });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ResolveGroupMarket_ByCreator_ReturnsError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"gmrc_{suffix}", $"gmrc_{suffix}@example.com");
        var groupId = await CreateGroup($"GM RC {suffix}", null, userId);
        await AddGroupMembership(userId, groupId);
        var marketId = await CreateGroupMarket(groupId, userId, $"GM RC Mkt {suffix}", "Desc", ["Yes", "No"]);
        var outcomeId = await GetOutcomeId(marketId);

        var response = await Client.PostAsJsonAsync($"/api/group-markets/{marketId}/resolve", new
        {
            marketId,
            winningOutcomeId = outcomeId,
            resolverId = userId
        });
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task CancelGroupMarket_WithValidData_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"gmcn_{suffix}", $"gmcn_{suffix}@example.com");
        var groupId = await CreateGroup($"GM Cancel {suffix}", null, userId);
        await AddGroupMembership(userId, groupId);
        var marketId = await CreateGroupMarket(groupId, userId, $"GM Cancel Mkt {suffix}", "Desc", ["Yes", "No"]);

        var response = await Client.PostAsJsonAsync($"/api/group-markets/{marketId}/cancel", new
        {
            marketId,
            requestingUserId = userId
        });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CancelGroupMarket_ByNonCreator_ReturnsError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var creatorId = await RegisterUser($"gmnc_{suffix}", $"gmnc_{suffix}@example.com");
        var groupId = await CreateGroup($"GM NC {suffix}", null, creatorId);
        await AddGroupMembership(creatorId, groupId);
        var marketId = await CreateGroupMarket(groupId, creatorId, $"GM NC Mkt {suffix}", "Desc", ["Yes", "No"]);

        var otherUserId = await RegisterUser($"gmnc2_{suffix}", $"gmnc2_{suffix}@example.com");

        var response = await Client.PostAsJsonAsync($"/api/group-markets/{marketId}/cancel", new
        {
            marketId,
            requestingUserId = otherUserId
        });
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
