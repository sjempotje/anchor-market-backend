using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Categories.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class CategoryTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task GetCategories_ReturnsList()
    {
        var response = await Client.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        Assert.NotNull(categories);
    }

    [Fact]
    public async Task CreateCategory_AsAdmin_ReturnsCreated()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await RegisterUser($"cat_{suffix}", $"cat_{suffix}@example.com");

        TestAuthHandler.IsAdmin = true;
        var response = await Client.PostAsJsonAsync("/api/categories", new
        {
            name = $"Crypto {suffix}",
            slug = $"crypto-{suffix}"
        });
        TestAuthHandler.IsAdmin = false;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetCategoryById_WithExistingId_ReturnsCategory()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await RegisterUser($"catid_{suffix}", $"catid_{suffix}@example.com");

        TestAuthHandler.IsAdmin = true;
        var createResponse = await Client.PostAsJsonAsync("/api/categories", new
        {
            name = $"Sports {suffix}",
            slug = $"sports-{suffix}"
        });
        TestAuthHandler.IsAdmin = false;

        var id = Guid.Parse(createResponse.Headers.Location!.Segments[^1]);

        var response = await Client.GetAsync($"/api/categories/{id}");
        response.EnsureSuccessStatusCode();

        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(category);
        Assert.Equal(id, category.Id);
        Assert.Equal($"Sports {suffix}", category.Name);
    }

    [Fact]
    public async Task GetCategoryById_WithNonExistentId_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/categories/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_AsAdmin_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await RegisterUser($"catdel_{suffix}", $"catdel_{suffix}@example.com");

        TestAuthHandler.IsAdmin = true;
        var createResponse = await Client.PostAsJsonAsync("/api/categories", new
        {
            name = $"Delete Me {suffix}",
            slug = $"delete-me-{suffix}"
        });
        var id = Guid.Parse(createResponse.Headers.Location!.Segments[^1]);

        var response = await Client.DeleteAsync($"/api/categories/{id}");
        TestAuthHandler.IsAdmin = false;

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithoutAdminRole_ReturnsForbidden()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await RegisterUser($"catna_{suffix}", $"catna_{suffix}@example.com");

        var response = await Client.PostAsJsonAsync("/api/categories", new
        {
            name = $"Unauthorized {suffix}",
            slug = $"unauthorized-{suffix}"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
