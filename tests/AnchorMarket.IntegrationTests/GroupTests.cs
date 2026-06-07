using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Groups.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

public class GroupTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task CreateGroup_ReturnsCreated()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"grp_{suffix}", $"grp_{suffix}@example.com");
        var groupId = await CreateGroup($"Test Group {suffix}", "A test group", userId);
        Assert.NotEqual(Guid.Empty, groupId);
    }

    [Fact]
    public async Task GetGroups_ReturnsList()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"grplist_{suffix}", $"grplist_{suffix}@example.com");
        await CreateGroup($"Group A {suffix}", null, userId);

        var response = await Client.GetAsync("/api/groups");
        response.EnsureSuccessStatusCode();

        var groups = await response.Content.ReadFromJsonAsync<List<GroupDto>>();
        Assert.NotNull(groups);
        Assert.Contains(groups, g => g.Name.Contains(suffix));
    }

    [Fact]
    public async Task GetGroup_WithExistingId_ReturnsGroup()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"grpid_{suffix}", $"grpid_{suffix}@example.com");
        var groupId = await CreateGroup($"Specific Group {suffix}", "Specific description", userId);

        var response = await Client.GetAsync($"/api/groups/{groupId}");
        response.EnsureSuccessStatusCode();

        var group = await response.Content.ReadFromJsonAsync<GroupDto>();
        Assert.NotNull(group);
        Assert.Equal(groupId, group.Id);
        Assert.Equal(userId, group.OwnerId);
    }

    [Fact]
    public async Task UpdateGroup_WithValidData_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"grpupd_{suffix}", $"grpupd_{suffix}@example.com");
        var groupId = await CreateGroup($"Original {suffix}", "Original description", userId);

        var response = await Client.PutAsJsonAsync($"/api/groups/{groupId}", new
        {
            groupId,
            name = $"Updated {suffix}",
            description = "Updated description"
        });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync($"/api/groups/{groupId}");
        var group = await getResponse.Content.ReadFromJsonAsync<GroupDto>();
        Assert.Equal($"Updated {suffix}", group!.Name);
    }

    [Fact]
    public async Task DeleteGroup_WithExistingId_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"grpdel_{suffix}", $"grpdel_{suffix}@example.com");
        var groupId = await CreateGroup($"Delete Me {suffix}", null, userId);

        var response = await Client.DeleteAsync($"/api/groups/{groupId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync($"/api/groups/{groupId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
