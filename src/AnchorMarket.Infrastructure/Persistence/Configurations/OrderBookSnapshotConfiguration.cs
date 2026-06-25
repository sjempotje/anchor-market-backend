using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="OrderBookSnapshot"/> entity.</summary>
public class OrderBookSnapshotConfiguration : IEntityTypeConfiguration<OrderBookSnapshot>
{
    /// <summary>Configures the <see cref="OrderBookSnapshot"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<OrderBookSnapshot> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Bids).IsRequired();
        builder.Property(s => s.Asks).IsRequired();

        builder.HasOne(s => s.Outcome)
            .WithMany()
            .HasForeignKey(s => s.OutcomeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.OutcomeId, s.Timestamp });
    }
}
