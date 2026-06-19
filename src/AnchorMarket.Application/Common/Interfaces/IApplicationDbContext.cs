using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    /// <summary>Limit orders placed by users for trading shares.</summary>
    DbSet<LimitOrder> LimitOrders { get; }

    /// <summary>Trade executions resulting from order matching.</summary>
    DbSet<TradeExecution> TradeExecutions { get; }

    /// <summary>User authentication sessions.</summary>
    DbSet<Session> Sessions { get; }
    /// <summary>User accounts (authentication identities).</summary>
    DbSet<Account> Accounts { get; }
    /// <summary>User verification records.</summary>
    DbSet<Verification> Verifications { get; }

    /// <summary>Taxonomy and discovery entities.</summary>
    DbSet<Category> Categories { get; }
    /// <summary>Calendar events associated with markets.</summary>
    DbSet<Event> Events { get; }

    /// <summary>Sports hierarchy entities.</summary>
    DbSet<Sport> Sports { get; }
    /// <summary>Sports leagues within a sport.</summary>
    DbSet<League> Leagues { get; }
    /// <summary>Sports teams.</summary>
    DbSet<Team> Teams { get; }
    /// <summary>Scheduled or completed sports matches.</summary>
    DbSet<Match> Matches { get; }
    /// <summary>Snapshots of match state over time.</summary>
    DbSet<MatchState> MatchStates { get; }
    /// <summary>Media streams associated with matches.</summary>
    DbSet<MatchStream> MatchStreams { get; }

    /// <summary>Market enrichment entities.</summary>
    DbSet<PriceHistory> PriceHistory { get; }
    /// <summary>Reusable templates for creating markets.</summary>
    DbSet<MarketTemplate> MarketTemplates { get; }

    /// <summary>Social and notification entities.</summary>
    DbSet<Comment> Comments { get; }
    /// <summary>User notifications.</summary>
    DbSet<Notification> Notifications { get; }
    /// <summary>Markets favorited by users.</summary>
    DbSet<FavoriteMarket> FavoriteMarkets { get; }
    /// <summary>Teams favorited by users.</summary>
    DbSet<FavoriteTeam> FavoriteTeams { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
