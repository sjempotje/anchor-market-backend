using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="OutcomePricePoint"/> entity.</summary>
public class OutcomePricePointConfiguration : IEntityTypeConfiguration<OutcomePricePoint>
{
    /// <summary>Configures the <see cref="OutcomePricePoint"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<OutcomePricePoint> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Price)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(p => p.Volume)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.HasOne(p => p.Outcome)
            .WithMany()
            .HasForeignKey(p => p.OutcomeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.OutcomeId, p.CreatedAt });
    }
}
