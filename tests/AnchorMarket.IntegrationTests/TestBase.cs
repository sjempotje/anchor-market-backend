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

    /// <summary>
    /// Seeds a user directly into the test database and sets the authenticated user
    /// to that user's ID for subsequent requests.
    /// </summary>
    protected async Task<Guid> RegisterUser(string username, string email)
    {
        using var db = Factory.CreateDbContext();
        var user = User.Create(username, email);
        db.Users.Add(user);
        var wallet = Wallet.Create(user.Id);
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();

        TestAuthHandler.CurrentUserId = user.Id;
        return user.Id;
    }

    protected async Task<Guid> CreateGroup(string name, string? description, Guid ownerId, bool isPrivate = false)
    {
        var response = await Client.PostAsJsonAsync("/api/groups", new { name, description, ownerId, isPrivate });
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

    protected async Task<Guid> CreateGroupMarket(Guid groupId, Guid creatorId, string title, string description, string[] outcomeTitles, Guid resolverId)
    {
        TestAuthHandler.CurrentUserId = creatorId;
        var response = await Client.PostAsJsonAsync("/api/group-markets", new
        {
            groupId,
            creatorId,
            title,
            description,
            resolutionDeadline = DateTimeOffset.UtcNow.AddDays(30),
            outcomeTitles,
            resolverId
        });
        if (!response.IsSuccessStatusCode)
            throw new Exception($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location!;
        return Guid.Parse(location.Segments[^1]);
    }

    protected async Task JoinGroup(Guid groupId, Guid userId, string? joinCode = null)
    {
        TestAuthHandler.CurrentUserId = userId;
        var response = await Client.PostAsJsonAsync($"/api/groups/{groupId}/join", new
        {
            joinCode
        });
        response.EnsureSuccessStatusCode();
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

    /// <summary>Adds a group membership if one doesn't already exist (e.g. the owner is auto-joined on creation).</summary>
    protected async Task AddGroupMembership(Guid userId, Guid groupId)
    {
        using var db = Factory.CreateDbContext();
        var exists = await db.GroupMemberships.AnyAsync(m => m.UserId == userId && m.GroupId == groupId);
        if (exists) return;

        var membership = GroupMembership.Create(userId, groupId);
        db.GroupMemberships.Add(membership);
        await db.SaveChangesAsync();
    }
}
