using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Teams.DTOs;
using AnchorMarket.Domain.Enums;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class TeamTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    private async Task<Guid> CreateSport(string suffix)
    {
        await RegisterUser($"tsport_{suffix}", $"tsport_{suffix}@example.com");
        TestAuthHandler.IsAdmin = true;
        var r = await Client.PostAsJsonAsync("/api/sports", new
        {
            name = $"Tennis {suffix}",
            slug = $"tennis-{suffix}",
            type = (int)SportType.Tennis
        });
        TestAuthHandler.IsAdmin = false;
        r.EnsureSuccessStatusCode();
        return Guid.Parse(r.Headers.Location!.Segments[^1]);
    }

    [Fact]
    public async Task GetTeams_ReturnsList()
    {
        var response = await Client.GetAsync("/api/teams");
        response.EnsureSuccessStatusCode();

        var teams = await response.Content.ReadFromJsonAsync<List<TeamDto>>();
        Assert.NotNull(teams);
    }

    [Fact]
    public async Task CreateTeam_WithValidData_ReturnsCreated()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var sportId = await CreateSport(suffix);

        TestAuthHandler.IsAdmin = true;
        var response = await Client.PostAsJsonAsync("/api/teams", new
        {
            name = $"FC Barcelona {suffix}",
            shortName = "FCB",
            slug = $"fc-barcelona-{suffix}",
            sportId
        });
        TestAuthHandler.IsAdmin = false;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = Guid.Parse(response.Headers.Location!.Segments[^1]);
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task GetTeamById_WithExistingId_ReturnsTeam()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var sportId = await CreateSport(suffix + "b");

        TestAuthHandler.IsAdmin = true;
        var createResponse = await Client.PostAsJsonAsync("/api/teams", new
        {
            name = $"Real Madrid {suffix}",
            shortName = "RM",
            slug = $"real-madrid-{suffix}",
            sportId
        });
        TestAuthHandler.IsAdmin = false;

        var id = Guid.Parse(createResponse.Headers.Location!.Segments[^1]);

        var response = await Client.GetAsync($"/api/teams/{id}");
        response.EnsureSuccessStatusCode();

        var team = await response.Content.ReadFromJsonAsync<TeamDto>();
        Assert.NotNull(team);
        Assert.Equal(id, team.Id);
        Assert.Equal($"Real Madrid {suffix}", team.Name);
    }

    [Fact]
    public async Task GetTeamById_WithNonExistentId_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/teams/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
