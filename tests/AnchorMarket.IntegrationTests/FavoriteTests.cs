using System.Net.Http.Json;
using AnchorMarket.Domain.Enums;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class FavoriteTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task ToggleFavoriteMarket_FirstCall_ReturnsFavorited()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"favmkt_{suffix}", $"favmkt_{suffix}@example.com");
        var marketId = await CreateMarket($"Fav Market {suffix}", "Desc", userId, ["Yes", "No"]);

        var response = await Client.PostAsync($"/api/favorites/markets/{marketId}", null);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
        Assert.NotNull(result);
        Assert.True(result["favorited"]);
    }

    [Fact]
    public async Task ToggleFavoriteMarket_SecondCall_ReturnsUnfavorited()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"favmkt2_{suffix}", $"favmkt2_{suffix}@example.com");
        var marketId = await CreateMarket($"Toggle Market {suffix}", "Desc", userId, ["Yes", "No"]);

        await Client.PostAsync($"/api/favorites/markets/{marketId}", null);
        var response = await Client.PostAsync($"/api/favorites/markets/{marketId}", null);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
        Assert.NotNull(result);
        Assert.False(result["favorited"]);
    }

    [Fact]
    public async Task ToggleFavoriteTeam_FirstCall_ReturnsFavorited()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await RegisterUser($"favteam_{suffix}", $"favteam_{suffix}@example.com");

        var sportResponse = await Client.PostAsJsonAsync("/api/sports", new
        {
            name = $"Sport {suffix}",
            slug = $"sport-{suffix}",
            type = (int)SportType.Soccer
        });
        var sportId = Guid.Parse(sportResponse.Headers.Location!.Segments[^1]);

        var teamResponse = await Client.PostAsJsonAsync("/api/teams", new
        {
            name = $"Team {suffix}",
            shortName = "TM",
            slug = $"team-{suffix}",
            sportId
        });
        var teamId = Guid.Parse(teamResponse.Headers.Location!.Segments[^1]);

        var response = await Client.PostAsync($"/api/favorites/teams/{teamId}", null);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
        Assert.NotNull(result);
        Assert.True(result["favorited"]);
    }
}
