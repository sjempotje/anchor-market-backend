using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="Outcome"/> entity.</summary>
public class OutcomeConfiguration : IEntityTypeConfiguration<Outcome>
{
    /// <summary>Configures the <see cref="Outcome"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Outcome> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(o => o.Positions)
            .WithOne(p => p.Outcome)
            .HasForeignKey(p => p.OutcomeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
