using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="FeedResult"/> entity.</summary>
public class FeedResultConfiguration : IEntityTypeConfiguration<FeedResult>
{
    /// <summary>Configures the <see cref="FeedResult"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<FeedResult> builder)
    {
        // Composite key includes the partition column (ReceivedAt) for PostgreSQL range partitioning.
        builder.HasKey(r => new { r.Id, r.ReceivedAt });

        builder.Property(r => r.RawJson)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasOne(r => r.Registration)
            .WithMany(f => f.Results)
            .HasForeignKey(r => r.FeedRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.FeedRegistrationId, r.ReceivedAt });
    }
}
