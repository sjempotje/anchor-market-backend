using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="TradeFlowSnapshot"/> entity.</summary>
public class TradeFlowSnapshotConfiguration : IEntityTypeConfiguration<TradeFlowSnapshot>
{
    /// <summary>Configures the <see cref="TradeFlowSnapshot"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<TradeFlowSnapshot> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Outcome)
            .WithMany()
            .HasForeignKey(s => s.OutcomeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.MarketId, s.Timestamp });
        builder.HasIndex(s => new { s.OutcomeId, s.Timestamp });
    }
}
