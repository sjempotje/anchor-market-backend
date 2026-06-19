using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Wallets.DTOs;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class WalletTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task GetWallet_WithExistingId_ReturnsWallet()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"wal_{suffix}", $"wal_{suffix}@example.com");

        var response = await Client.GetAsync($"/api/wallets/user/{userId}");
        response.EnsureSuccessStatusCode();

        var wallet = await response.Content.ReadFromJsonAsync<WalletDto>();
        Assert.NotNull(wallet);
        Assert.Equal(userId, wallet.UserId);
    }

    [Fact]
    public async Task GetWallet_WithNonExistentId_ReturnsNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        TestAuthHandler.CurrentUserId = nonExistentId;
        var response = await Client.GetAsync($"/api/wallets/user/{nonExistentId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetWalletTransactions_ReturnsList()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"waltr_{suffix}", $"waltr_{suffix}@example.com");

        var response = await Client.GetAsync($"/api/wallets/user/{userId}/transactions");
        response.EnsureSuccessStatusCode();

        var transactions = await response.Content.ReadFromJsonAsync<List<TransactionDto>>();
        Assert.NotNull(transactions);
    }

    [Fact]
    public async Task GetWalletTransactions_WithNonExistentId_ReturnsEmptyList()
    {
        var nonExistentId = Guid.NewGuid();
        TestAuthHandler.CurrentUserId = nonExistentId;
        var response = await Client.GetAsync($"/api/wallets/user/{nonExistentId}/transactions");
        response.EnsureSuccessStatusCode();

        var transactions = await response.Content.ReadFromJsonAsync<List<TransactionDto>>();
        Assert.NotNull(transactions);
        Assert.Empty(transactions);
    }
}
