using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AnchorMarket.IntegrationTests;

public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected TestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var db = Factory.CreateDbContext();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<Guid> RegisterUser(string username, string email)
    {
        var response = await Client.PostAsJsonAsync("/api/users/register", new { username, email });
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location!;
        return Guid.Parse(location.Segments[^1]);
    }

    protected async Task<Guid> CreateGroup(string name, string? description, Guid ownerId)
    {
        var response = await Client.PostAsJsonAsync("/api/groups", new { name, description, ownerId });
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location!;
        return Guid.Parse(location.Segments[^1]);
    }

    protected async Task<Guid> CreateMarket(string title, string description, Guid creatorId, string[] outcomeTitles, Guid? groupId = null)
    {
        var response = await Client.PostAsJsonAsync("/api/markets", new
        {
            title,
            description,
            resolutionDeadline = DateTimeOffset.UtcNow.AddDays(30),
            scope = groupId.HasValue ? 1 : 0,
            creatorId,
            groupId,
            outcomeTitles
        });
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location!;
        return Guid.Parse(location.Segments[^1]);
    }

    protected async Task<Guid> CreateGroupMarket(Guid groupId, Guid creatorId, string title, string description, string[] outcomeTitles)
    {
        var response = await Client.PostAsJsonAsync("/api/group-markets", new
        {
            groupId,
            creatorId,
            title,
            description,
            resolutionDeadline = DateTimeOffset.UtcNow.AddDays(30),
            outcomeTitles
        });
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location!;
        return Guid.Parse(location.Segments[^1]);
    }

    protected async Task<Guid> PlaceLimitOrder(Guid userId, Guid marketId, Guid outcomeId, int side = 0, decimal price = 0.55m, decimal quantity = 100.0m)
    {
        var response = await Client.PostAsJsonAsync("/api/limitorders", new
        {
            userId,
            marketId,
            outcomeId,
            side,
            price,
            quantity,
            expiresAt = (DateTimeOffset?)DateTimeOffset.UtcNow.AddDays(7)
        });
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return Guid.Parse(response.Headers.Location!.Segments[^1]);
    }

    protected async Task<Guid> GetOutcomeId(Guid marketId)
    {
        using var db = Factory.CreateDbContext();
        var market = await db.Markets.Include(m => m.Outcomes).FirstAsync(m => m.Id == marketId);
        return market.Outcomes.First().Id;
    }

    protected async Task<Dictionary<string, Guid>> GetOutcomes(Guid marketId)
    {
        using var db = Factory.CreateDbContext();
        var market = await db.Markets.Include(m => m.Outcomes).FirstAsync(m => m.Id == marketId);
        return market.Outcomes.ToDictionary(o => o.Title, o => o.Id);
    }

    protected async Task<Guid> GetWalletId(Guid userId)
    {
        using var db = Factory.CreateDbContext();
        var wallet = await db.Wallets.FirstAsync(w => w.UserId == userId);
        return wallet.Id;
    }

    protected async Task CreditWallet(Guid userId, decimal amount)
    {
        using var db = Factory.CreateDbContext();
        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet is null)
        {
            wallet = Wallet.Create(userId);
            db.Wallets.Add(wallet);
        }
        wallet.Credit(amount);
        await db.SaveChangesAsync();
    }

    protected async Task AddGroupMembership(Guid userId, Guid groupId)
    {
        using var db = Factory.CreateDbContext();
        var membership = GroupMembership.Create(userId, groupId);
        db.GroupMemberships.Add(membership);
        await db.SaveChangesAsync();
    }
}
