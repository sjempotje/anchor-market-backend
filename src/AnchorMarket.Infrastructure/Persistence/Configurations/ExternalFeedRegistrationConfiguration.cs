using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="ExternalFeedRegistration"/> entity.</summary>
public class ExternalFeedRegistrationConfiguration : IEntityTypeConfiguration<ExternalFeedRegistration>
{
    /// <summary>Configures the <see cref="ExternalFeedRegistration"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<ExternalFeedRegistration> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.AdapterType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Config)
            .IsRequired();

        builder.Property(f => f.ApiUrl)
            .HasMaxLength(2000);

        builder.Property(f => f.AuthToken)
            .HasMaxLength(2000);

        builder.HasOne(f => f.Market)
            .WithMany()
            .HasForeignKey(f => f.MarketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.MarketId, f.IsActive });
    }
}
