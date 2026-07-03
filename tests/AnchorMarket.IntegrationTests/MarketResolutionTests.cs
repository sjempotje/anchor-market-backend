using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Application.Features.MarketResolutions.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class MarketResolutionTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task ResolvePublicMarket_AsAdmin_ResolvesAndExposesWinner()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var creator = await RegisterUser($"res_{suffix}", $"res_{suffix}@x.com");
        var marketId = await CreateMarket($"Price Movement {suffix}", "Up or down?", creator, ["UP", "DOWN"]);
        var outcomes = await GetOutcomes(marketId);
        var upId = outcomes["UP"];

        TestAuthHandler.IsAdmin = true;
        var resolve = await Client.PostAsJsonAsync($"/api/markets/{marketId}/resolve", new { winningOutcomeId = upId });
        TestAuthHandler.IsAdmin = false;
        Assert.Equal(HttpStatusCode.NoContent, resolve.StatusCode);

        var resolutionResponse = await Client.GetAsync($"/api/markets/{marketId}/resolution");
        resolutionResponse.EnsureSuccessStatusCode();
        var resolution = await resolutionResponse.Content.ReadFromJsonAsync<MarketResolutionDto>();
        Assert.NotNull(resolution);
        Assert.Equal(upId, resolution.WinningOutcomeId);

        var market = await Client.GetFromJsonAsync<MarketDto>($"/api/markets/{marketId}");
        Assert.NotNull(market);
        Assert.Equal(Domain.Enums.MarketStatus.Resolved, market.Status);
    }

    [Fact]
    public async Task ResolvePublicMarket_AsNonAdmin_ReturnsForbidden()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var creator = await RegisterUser($"resn_{suffix}", $"resn_{suffix}@x.com");
        var marketId = await CreateMarket($"Price Movement {suffix}", "Up or down?", creator, ["UP", "DOWN"]);
        var outcomes = await GetOutcomes(marketId);

        var resolve = await Client.PostAsJsonAsync($"/api/markets/{marketId}/resolve", new { winningOutcomeId = outcomes["UP"] });

        Assert.Equal(HttpStatusCode.Forbidden, resolve.StatusCode);
    }

    [Fact]
    public async Task GetResolution_UnresolvedMarket_ReturnsNotFound()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var creator = await RegisterUser($"resu_{suffix}", $"resu_{suffix}@x.com");
        var marketId = await CreateMarket($"Price Movement {suffix}", "Up or down?", creator, ["UP", "DOWN"]);

        var response = await Client.GetAsync($"/api/markets/{marketId}/resolution");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ResolvePublicMarket_OnGroupMarket_IsRejected()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var owner = await RegisterUser($"resg_{suffix}", $"resg_{suffix}@x.com");
        var groupId = await CreateGroup($"Grp {suffix}", "g", owner);
        await AddGroupMembership(owner, groupId);
        var resolverId = await RegisterUser($"resgres_{suffix}", $"resgres_{suffix}@x.com");
        await AddGroupMembership(resolverId, groupId);
        var marketId = await CreateGroupMarket(groupId, owner, $"Grp Market {suffix}", "g", ["UP", "DOWN"], resolverId);
        var outcomes = await GetOutcomes(marketId);

        TestAuthHandler.IsAdmin = true;
        var resolve = await Client.PostAsJsonAsync($"/api/markets/{marketId}/resolve", new { winningOutcomeId = outcomes["UP"] });
        TestAuthHandler.IsAdmin = false;

        // Group markets must be resolved via the group endpoint; the public endpoint rejects them.
        Assert.Equal(HttpStatusCode.BadRequest, resolve.StatusCode);
    }
}
