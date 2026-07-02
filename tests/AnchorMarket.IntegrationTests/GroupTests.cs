using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Groups.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
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

    [Fact]
    public async Task CreatePrivateGroup_HasJoinCode()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"privgrp_{suffix}", $"privgrp_{suffix}@example.com");

        var response = await Client.PostAsJsonAsync("/api/groups", new
        {
            name = $"Private Group {suffix}",
            description = "A private group",
            ownerId = userId,
            isPrivate = true
        });
        response.EnsureSuccessStatusCode();

        var groupId = Guid.Parse(response.Headers.Location!.Segments[^1]);
        var getResponse = await Client.GetAsync($"/api/groups/{groupId}");
        var group = await getResponse.Content.ReadFromJsonAsync<GroupDto>();

        Assert.NotNull(group);
        Assert.True(group!.IsPrivate);
        Assert.NotNull(group.JoinCode);
        Assert.NotEmpty(group.JoinCode);
    }

    [Fact]
    public async Task JoinPrivateGroup_WithValidCode_Succeeds()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var ownerId = await RegisterUser($"privown_{suffix}", $"privown_{suffix}@example.com");

        // Create private group
        var groupResponse = await Client.PostAsJsonAsync("/api/groups", new
        {
            name = $"Private {suffix}",
            description = "A private group",
            ownerId,
            isPrivate = true
        });
        var groupId = Guid.Parse(groupResponse.Headers.Location!.Segments[^1]);

        // Get the group to get the join code
        var getGroupResponse = await Client.GetAsync($"/api/groups/{groupId}");
        var groupDto = await getGroupResponse.Content.ReadFromJsonAsync<GroupDto>();
        var joinCode = groupDto!.JoinCode;

        // Join with correct code
        var userId = await RegisterUser($"joiner_{suffix}", $"joiner_{suffix}@example.com");
        await JoinGroup(groupId, userId, joinCode);

        // Verify membership
        var response = await Client.GetAsync($"/api/group-markets?groupId={groupId}&requestingUserId={userId}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task JoinPrivateGroup_WithInvalidCode_Fails()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var ownerId = await RegisterUser($"privfail_{suffix}", $"privfail_{suffix}@example.com");

        var groupResponse = await Client.PostAsJsonAsync("/api/groups", new
        {
            name = $"Private {suffix}",
            description = "A private group",
            ownerId,
            isPrivate = true
        });
        var groupId = Guid.Parse(groupResponse.Headers.Location!.Segments[^1]);

        var userId = await RegisterUser($"joinfail_{suffix}", $"joinfail_{suffix}@example.com");
        TestAuthHandler.CurrentUserId = userId;

        var joinResponse = await Client.PostAsJsonAsync($"/api/groups/{groupId}/join", new
        {
            joinCode = "WRONGCODE"
        });
        Assert.Equal(HttpStatusCode.BadRequest, joinResponse.StatusCode);
    }

    [Fact]
    public async Task JoinPublicGroup_WithoutCode_Succeeds()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var ownerId = await RegisterUser($"pubown_{suffix}", $"pubown_{suffix}@example.com");

        var groupResponse = await Client.PostAsJsonAsync("/api/groups", new
        {
            name = $"Public {suffix}",
            description = "A public group",
            ownerId,
            isPrivate = false
        });
        var groupId = Guid.Parse(groupResponse.Headers.Location!.Segments[^1]);

        var userId = await RegisterUser($"pubjoiner_{suffix}", $"pubjoiner_{suffix}@example.com");
        await JoinGroup(groupId, userId);

        // Verify membership
        var response = await Client.GetAsync($"/api/group-markets?groupId={groupId}&requestingUserId={userId}");
        response.EnsureSuccessStatusCode();
    }
}
