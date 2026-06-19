using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Market> Markets => Set<Market>();
    public DbSet<Outcome> Outcomes => Set<Outcome>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMembership> GroupMemberships => Set<GroupMembership>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<MarketResolution> MarketResolutions => Set<MarketResolution>();

    /// <summary>Limit orders placed by users for trading shares.</summary>
    public DbSet<LimitOrder> LimitOrders => Set<LimitOrder>();

    /// <summary>Trade executions resulting from order matching.</summary>
    public DbSet<TradeExecution> TradeExecutions => Set<TradeExecution>();

    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Verification> Verifications => Set<Verification>();

    // <summary>Taxonomy & discovery</summary>
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Event> Events => Set<Event>();

    // <summary>Sports hierarchy</summary>
    public DbSet<Sport> Sports => Set<Sport>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchState> MatchStates => Set<MatchState>();
    public DbSet<MatchStream> MatchStreams => Set<MatchStream>();

    // <summary>Market enrichment</summary>
    public DbSet<PriceHistory> PriceHistory => Set<PriceHistory>();
    public DbSet<MarketTemplate> MarketTemplates => Set<MarketTemplate>();

    // <summary>Social & notifications</summary>
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<FavoriteMarket> FavoriteMarkets => Set<FavoriteMarket>();
    public DbSet<FavoriteTeam> FavoriteTeams => Set<FavoriteTeam>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
