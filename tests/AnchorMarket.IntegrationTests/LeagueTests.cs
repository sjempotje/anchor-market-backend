using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Leagues.DTOs;
using AnchorMarket.Domain.Enums;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class LeagueTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    private async Task<Guid> CreateSport(string suffix)
    {
        await RegisterUser($"lsport_{suffix}", $"lsport_{suffix}@example.com");
        TestAuthHandler.IsAdmin = true;
        var r = await Client.PostAsJsonAsync("/api/sports", new
        {
            name = $"Soccer {suffix}",
            slug = $"soccer-{suffix}",
            type = (int)SportType.Soccer
        });
        TestAuthHandler.IsAdmin = false;
        r.EnsureSuccessStatusCode();
        return Guid.Parse(r.Headers.Location!.Segments[^1]);
    }

    [Fact]
    public async Task GetLeagues_ReturnsList()
    {
        var response = await Client.GetAsync("/api/leagues");
        response.EnsureSuccessStatusCode();

        var leagues = await response.Content.ReadFromJsonAsync<List<LeagueDto>>();
        Assert.NotNull(leagues);
    }

    [Fact]
    public async Task CreateLeague_AsAdmin_ReturnsCreated()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var sportId = await CreateSport(suffix);

        TestAuthHandler.IsAdmin = true;
        var response = await Client.PostAsJsonAsync("/api/leagues", new
        {
            name = $"Premier League {suffix}",
            slug = $"premier-league-{suffix}",
            sportId
        });
        TestAuthHandler.IsAdmin = false;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetLeagueById_WithExistingId_ReturnsLeague()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var sportId = await CreateSport(suffix + "b");

        TestAuthHandler.IsAdmin = true;
        var createResponse = await Client.PostAsJsonAsync("/api/leagues", new
        {
            name = $"La Liga {suffix}",
            slug = $"la-liga-{suffix}",
            sportId
        });
        TestAuthHandler.IsAdmin = false;

        var id = Guid.Parse(createResponse.Headers.Location!.Segments[^1]);

        var response = await Client.GetAsync($"/api/leagues/{id}");
        response.EnsureSuccessStatusCode();

        var league = await response.Content.ReadFromJsonAsync<LeagueDto>();
        Assert.NotNull(league);
        Assert.Equal(id, league.Id);
    }

    [Fact]
    public async Task GetLeagueById_WithNonExistentId_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/leagues/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateLeague_WithoutAdminRole_ReturnsForbidden()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var sportId = await CreateSport(suffix + "c");

        var response = await Client.PostAsJsonAsync("/api/leagues", new
        {
            name = $"Bundesliga {suffix}",
            slug = $"bundesliga-{suffix}",
            sportId
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
