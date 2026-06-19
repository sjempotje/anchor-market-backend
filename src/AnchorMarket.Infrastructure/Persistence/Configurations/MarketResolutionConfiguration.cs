using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="MarketResolution"/> entity.</summary>
public class MarketResolutionConfiguration : IEntityTypeConfiguration<MarketResolution>
{
    /// <summary>Configures the <see cref="MarketResolution"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
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
