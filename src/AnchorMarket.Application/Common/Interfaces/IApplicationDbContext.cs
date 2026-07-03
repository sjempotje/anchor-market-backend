using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace AnchorMarket.Application.Common.Interfaces;

/// <summary>Abstraction for the application's database context exposing entity DbSets and save functionality.</summary>
public interface IApplicationDbContext
{
    /// <summary>Creates a DbSet for the specified entity type.</summary>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>Application user accounts.</summary>
    DbSet<User> Users { get; }
    /// <summary>Prediction markets created by users.</summary>
    DbSet<Market> Markets { get; }
    /// <summary>Possible outcomes for a market.</summary>
    DbSet<Outcome> Outcomes { get; }
    /// <summary>User-created groups for social trading.</summary>
    DbSet<Group> Groups { get; }
    /// <summary>Membership records linking users to groups.</summary>
    DbSet<GroupMembership> GroupMemberships { get; }
    /// <summary>User positions (holdings) in market outcomes.</summary>
    DbSet<Position> Positions { get; }
    /// <summary>User wallets for managing balances.</summary>
    DbSet<Wallet> Wallets { get; }
    /// <summary>Financial transactions recorded against wallets.</summary>
    DbSet<Transaction> Transactions { get; }
    /// <summary>Resolution records for settled markets.</summary>
    DbSet<MarketResolution> MarketResolutions { get; }
    /// <summary>Historical implied-probability prices sampled on each trade.</summary>
    DbSet<OutcomePricePoint> OutcomePricePoints { get; }

    /// <summary>User authentication sessions.</summary>
    DbSet<Session> Sessions { get; }
    /// <summary>User accounts (authentication identities).</summary>
    DbSet<Account> Accounts { get; }
    /// <summary>User verification records.</summary>
    DbSet<Verification> Verifications { get; }

    /// <summary>Taxonomy and discovery entities.</summary>
    DbSet<Category> Categories { get; }

    /// <summary>Reusable templates for creating markets.</summary>
    DbSet<MarketTemplate> MarketTemplates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Exposes database operations (transactions, migrations) to application handlers.</summary>
    DatabaseFacade Database { get; }

    /// <summary>Begins a transaction at the specified isolation level. Falls back to default isolation on providers that don't support it (e.g. SQLite).</summary>
    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
}
