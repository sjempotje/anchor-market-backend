using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

public class MarketResolutionConfiguration : IEntityTypeConfiguration<MarketResolution>
{
    public void Configure(EntityTypeBuilder<MarketResolution> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.MarketId)
            .IsUnique();

        builder.Property(r => r.ResolvedById)
            .IsRequired();

        builder.HasOne(r => r.WinningOutcome)
            .WithMany()
            .HasForeignKey(r => r.WinningOutcomeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
