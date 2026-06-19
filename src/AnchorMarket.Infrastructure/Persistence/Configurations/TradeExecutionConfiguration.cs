using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="TradeExecution"/> entity.</summary>
public class TradeExecutionConfiguration : IEntityTypeConfiguration<TradeExecution>
{
    /// <summary>Configures the <see cref="TradeExecution"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<TradeExecution> builder)
    {
        builder.HasKey(te => te.Id);

        builder.Property(te => te.LimitOrderId)
            .IsRequired();

        builder.Property(te => te.MarketId)
            .IsRequired();

        builder.Property(te => te.OutcomeId)
            .IsRequired();

        builder.Property(te => te.BuyerOrderId)
            .IsRequired();

        builder.Property(te => te.SellerOrderId)
            .IsRequired();

        builder.Property(te => te.InitiatorUserId)
            .IsRequired();

        builder.Property(te => te.Shares)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(te => te.ExecutedPrice)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(te => te.TotalValue)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.HasOne(te => te.Market)
            .WithMany()
            .HasForeignKey(te => te.MarketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(te => te.Outcome)
            .WithMany()
            .HasForeignKey(te => te.OutcomeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
