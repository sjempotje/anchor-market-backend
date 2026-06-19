using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="Market"/> entity.</summary>
public class MarketConfiguration : IEntityTypeConfiguration<Market>
{
    /// <summary>Configures the <see cref="Market"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Market> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.Scope)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.CreatorId)
            .IsRequired();

        builder.HasOne(m => m.Group)
            .WithMany(g => g.Markets)
            .HasForeignKey(m => m.GroupId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Resolution)
            .WithOne(r => r.Market)
            .HasForeignKey<MarketResolution>(r => r.MarketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Outcomes)
            .WithOne(o => o.Market)
            .HasForeignKey(o => o.MarketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
