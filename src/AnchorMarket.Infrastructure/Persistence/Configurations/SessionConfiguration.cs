using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="Session"/> entity.</summary>
public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    /// <summary>Configures the <see cref="Session"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(s => s.ExpiresAt)
            .IsRequired();

        builder.Property(s => s.IpAddress)
            .HasMaxLength(45);

        builder.Property(s => s.UserAgent)
            .HasMaxLength(500);

        builder.HasIndex(s => s.Token)
            .IsUnique()
            .HasDatabaseName("IX_Sessions_Token");

        builder.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
