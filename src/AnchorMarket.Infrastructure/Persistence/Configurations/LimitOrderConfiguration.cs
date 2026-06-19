using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="LimitOrder"/> entity.</summary>
public class LimitOrderConfiguration : IEntityTypeConfiguration<LimitOrder>
{
    /// <summary>Configures the <see cref="LimitOrder"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<LimitOrder> builder)
    {
        builder.HasKey(lo => lo.Id);

        builder.Property(lo => lo.MarketId)
            .IsRequired();

        builder.Property(lo => lo.OutcomeId)
            .IsRequired();

        builder.Property(lo => lo.UserId)
            .IsRequired();

        builder.Property(lo => lo.Side)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(lo => lo.Price)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(lo => lo.Quantity)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(lo => lo.FilledQuantity)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(lo => lo.TotalCost)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(lo => lo.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(lo => lo.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(lo => lo.ExpiresAt)
            .IsRequired(false);

        builder.HasOne(lo => lo.Market)
            .WithMany()
            .HasForeignKey(lo => lo.MarketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lo => lo.Outcome)
            .WithMany()
            .HasForeignKey(lo => lo.OutcomeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(lo => lo.TradeExecutions)
            .WithOne(te => te.LimitOrder)
            .HasForeignKey(te => te.LimitOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
