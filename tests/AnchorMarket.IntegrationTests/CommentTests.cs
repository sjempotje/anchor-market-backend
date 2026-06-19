using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Comments.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class CommentTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task GetComments_ForMarket_ReturnsList()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"cmtlist_{suffix}", $"cmtlist_{suffix}@example.com");
        var marketId = await CreateMarket($"Comment Market {suffix}", "Desc", userId, ["Yes", "No"]);

        var response = await Client.GetAsync($"/api/markets/{marketId}/comments");
        response.EnsureSuccessStatusCode();

        var comments = await response.Content.ReadFromJsonAsync<List<CommentDto>>();
        Assert.NotNull(comments);
    }

    [Fact]
    public async Task CreateComment_WithValidData_ReturnsOk()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"cmtcreate_{suffix}", $"cmtcreate_{suffix}@example.com");
        var marketId = await CreateMarket($"Comment Create Market {suffix}", "Desc", userId, ["Yes", "No"]);

        var response = await Client.PostAsJsonAsync($"/api/markets/{marketId}/comments", new
        {
            marketId,
            userId,
            content = "This is a test comment"
        });

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateComment_ThenGetComments_ContainsComment()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"cmtget_{suffix}", $"cmtget_{suffix}@example.com");
        var marketId = await CreateMarket($"Comment Get Market {suffix}", "Desc", userId, ["Yes", "No"]);

        await Client.PostAsJsonAsync($"/api/markets/{marketId}/comments", new
        {
            marketId,
            userId,
            content = "Hello from test"
        });

        var response = await Client.GetAsync($"/api/markets/{marketId}/comments");
        response.EnsureSuccessStatusCode();

        var comments = await response.Content.ReadFromJsonAsync<List<CommentDto>>();
        Assert.NotNull(comments);
        Assert.Contains(comments, c => c.Content == "Hello from test");
    }

    [Fact]
    public async Task DeleteComment_AsAuthor_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"cmtdel_{suffix}", $"cmtdel_{suffix}@example.com");
        var marketId = await CreateMarket($"Comment Del Market {suffix}", "Desc", userId, ["Yes", "No"]);

        var createResponse = await Client.PostAsJsonAsync($"/api/markets/{marketId}/comments", new
        {
            marketId,
            userId,
            content = "To be deleted"
        });
        var body = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var commentId = body!["id"];

        var response = await Client.DeleteAsync($"/api/markets/{marketId}/comments/{commentId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
