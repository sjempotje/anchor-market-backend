using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Sports.DTOs;
using AnchorMarket.Domain.Enums;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class SportsTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task GetSports_ReturnsList()
    {
        var response = await Client.GetAsync("/api/sports");
        response.EnsureSuccessStatusCode();

        var sports = await response.Content.ReadFromJsonAsync<List<SportDto>>();
        Assert.NotNull(sports);
    }

    [Fact]
    public async Task CreateSport_WithValidData_ReturnsCreated()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await RegisterUser($"sports_{suffix}", $"sports_{suffix}@example.com");

        var response = await Client.PostAsJsonAsync("/api/sports", new
        {
            name = $"Football {suffix}",
            slug = $"football-{suffix}",
            type = (int)SportType.Soccer
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = Guid.Parse(response.Headers.Location!.Segments[^1]);
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task GetSportById_WithExistingId_ReturnsSport()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await RegisterUser($"sportsid_{suffix}", $"sportsid_{suffix}@example.com");

        var createResponse = await Client.PostAsJsonAsync("/api/sports", new
        {
            name = $"Basketball {suffix}",
            slug = $"basketball-{suffix}",
            type = (int)SportType.Basketball
        });

        var id = Guid.Parse(createResponse.Headers.Location!.Segments[^1]);

        var response = await Client.GetAsync($"/api/sports/{id}");
        response.EnsureSuccessStatusCode();

        var sport = await response.Content.ReadFromJsonAsync<SportDto>();
        Assert.NotNull(sport);
        Assert.Equal(id, sport.Id);
        Assert.Equal($"Basketball {suffix}", sport.Name);
    }

    [Fact]
    public async Task GetSportById_WithNonExistentId_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/sports/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
