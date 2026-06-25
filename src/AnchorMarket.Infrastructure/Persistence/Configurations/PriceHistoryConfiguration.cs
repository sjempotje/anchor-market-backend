using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="PriceHistory"/> entity.</summary>
public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    /// <summary>Configures the <see cref="PriceHistory"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        // Composite key includes the partition column (Timestamp) for PostgreSQL range partitioning.
        builder.HasKey(p => new { p.Id, p.Timestamp });

        builder.HasIndex(p => new { p.OutcomeId, p.Timestamp });
    }
}
