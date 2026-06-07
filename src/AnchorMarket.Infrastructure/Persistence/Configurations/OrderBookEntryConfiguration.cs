using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

public class OrderBookEntryConfiguration : IEntityTypeConfiguration<OrderBookEntry>
{
    public void Configure(EntityTypeBuilder<OrderBookEntry> builder)
    {
        builder.HasKey(obe => obe.Id);

        builder.HasIndex(obe => new { obe.MarketId, obe.OutcomeId, obe.Price, obe.Side })
            .IsUnique();

        builder.Property(obe => obe.MarketId)
            .IsRequired();

        builder.Property(obe => obe.OutcomeId)
            .IsRequired();

        builder.Property(obe => obe.Price)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(obe => obe.TotalQuantity)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(obe => obe.OrderCount)
            .IsRequired();

        builder.Property(obe => obe.Side)
            .HasConversion<string>()
            .IsRequired();

        builder.HasOne(obe => obe.Market)
            .WithMany()
            .HasForeignKey(obe => obe.MarketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(obe => obe.Outcome)
            .WithMany()
            .HasForeignKey(obe => obe.OutcomeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
