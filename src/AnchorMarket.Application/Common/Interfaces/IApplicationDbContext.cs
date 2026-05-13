using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    DbSet<Product> Products { get; }
    DbSet<Market> Markets { get; }
    DbSet<Outcome> Outcomes { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupMembership> GroupMemberships { get; }
    DbSet<Position> Positions { get; }
    DbSet<Wallet> Wallets { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<MarketResolution> MarketResolutions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
