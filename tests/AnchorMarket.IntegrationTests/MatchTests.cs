using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Matches.DTOs;
using AnchorMarket.Domain.Enums;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class MatchTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    private async Task<(Guid sportId, Guid leagueId, Guid homeTeamId, Guid awayTeamId)> SetupMatchPrerequisites(string suffix)
    {
        await RegisterUser($"msetup_{suffix}", $"msetup_{suffix}@example.com");

        TestAuthHandler.IsAdmin = true;
        var sportResponse = await Client.PostAsJsonAsync("/api/sports", new
        {
            name = $"Match Sport {suffix}",
            slug = $"match-sport-{suffix}",
            type = (int)SportType.Soccer
        });
        var sportId = Guid.Parse(sportResponse.Headers.Location!.Segments[^1]);

        var leagueResponse = await Client.PostAsJsonAsync("/api/leagues", new
        {
            name = $"Match League {suffix}",
            slug = $"match-league-{suffix}",
            sportId
        });
        var leagueId = Guid.Parse(leagueResponse.Headers.Location!.Segments[^1]);

        var homeResponse = await Client.PostAsJsonAsync("/api/teams", new
        {
            name = $"Home Team {suffix}",
            shortName = "HM",
            slug = $"home-{suffix}",
            sportId
        });
        var homeTeamId = Guid.Parse(homeResponse.Headers.Location!.Segments[^1]);

        var awayResponse = await Client.PostAsJsonAsync("/api/teams", new
        {
            name = $"Away Team {suffix}",
            shortName = "AW",
            slug = $"away-{suffix}",
            sportId
        });
        var awayTeamId = Guid.Parse(awayResponse.Headers.Location!.Segments[^1]);
        TestAuthHandler.IsAdmin = false;

        return (sportId, leagueId, homeTeamId, awayTeamId);
    }

    [Fact]
    public async Task GetMatches_ReturnsList()
    {
        var response = await Client.GetAsync("/api/matches");
        response.EnsureSuccessStatusCode();

        var matches = await response.Content.ReadFromJsonAsync<List<MatchDto>>();
        Assert.NotNull(matches);
    }

    [Fact]
    public async Task CreateMatch_AsAdmin_ReturnsCreated()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (_, leagueId, homeTeamId, awayTeamId) = await SetupMatchPrerequisites(suffix);

        TestAuthHandler.IsAdmin = true;
        var response = await Client.PostAsJsonAsync("/api/matches", new
        {
            homeTeamId,
            awayTeamId,
            leagueId,
            startTime = DateTimeOffset.UtcNow.AddDays(7)
        });
        TestAuthHandler.IsAdmin = false;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetMatchById_WithExistingId_ReturnsMatch()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (_, leagueId, homeTeamId, awayTeamId) = await SetupMatchPrerequisites(suffix + "b");

        TestAuthHandler.IsAdmin = true;
        var createResponse = await Client.PostAsJsonAsync("/api/matches", new
        {
            homeTeamId,
            awayTeamId,
            leagueId,
            startTime = DateTimeOffset.UtcNow.AddDays(7)
        });
        TestAuthHandler.IsAdmin = false;

        var id = Guid.Parse(createResponse.Headers.Location!.Segments[^1]);

        var response = await Client.GetAsync($"/api/matches/{id}");
        response.EnsureSuccessStatusCode();

        var match = await response.Content.ReadFromJsonAsync<MatchDto>();
        Assert.NotNull(match);
        Assert.Equal(id, match.Id);
        Assert.Equal(homeTeamId, match.HomeTeamId);
        Assert.Equal(awayTeamId, match.AwayTeamId);
    }

    [Fact]
    public async Task GetMatchById_WithNonExistentId_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/matches/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateMatch_WithoutAdminRole_ReturnsForbidden()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (_, leagueId, homeTeamId, awayTeamId) = await SetupMatchPrerequisites(suffix + "c");

        var response = await Client.PostAsJsonAsync("/api/matches", new
        {
            homeTeamId,
            awayTeamId,
            leagueId,
            startTime = DateTimeOffset.UtcNow.AddDays(7)
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
