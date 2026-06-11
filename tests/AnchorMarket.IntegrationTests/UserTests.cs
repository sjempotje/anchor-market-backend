using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Users.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class UserTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task RegisterUser_CreatesUser_ReturnsCreated()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"user_{suffix}", $"user_{suffix}@example.com");
        Assert.NotEqual(Guid.Empty, userId);
    }

    [Fact]
    public async Task RegisterUser_DuplicateUsername_ReturnsError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await RegisterUser($"dup_{suffix}", $"dup_{suffix}@example.com");

        var response = await Client.PostAsJsonAsync("/api/users/register", new
        {
            username = $"dup_{suffix}",
            email = $"other_{suffix}@example.com"
        });
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetUser_WithExistingId_ReturnsUser()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"get_{suffix}", $"get_{suffix}@example.com");

        var response = await Client.GetAsync($"/api/users/{userId}");
        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal($"get_{suffix}", user.Username);
    }

    [Fact]
    public async Task GetUser_WithNonExistentId_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/users/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"upd_{suffix}", $"upd_{suffix}@example.com");

        var response = await Client.PutAsJsonAsync($"/api/users/{userId}", new
        {
            userId,
            username = $"upd_{suffix}_renamed",
            email = $"upd_{suffix}_renamed@example.com"
        });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync($"/api/users/{userId}");
        var user = await getResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal($"upd_{suffix}_renamed", user!.Username);
    }

    [Fact]
    public async Task DeleteUser_WithExistingId_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"del_{suffix}", $"del_{suffix}@example.com");

        var response = await Client.DeleteAsync($"/api/users/{userId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync($"/api/users/{userId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
