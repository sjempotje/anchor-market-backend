using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="Position"/> entity.</summary>
public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    /// <summary>Configures the <see cref="Position"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(p => p.Shares)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(p => p.EntryPrice)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(p => p.FairValueAtEntry)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(p => p.CurrentFairValue)
            .HasPrecision(18, 6)
            .IsRequired();
    }
}
