using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

public class VerificationConfiguration : IEntityTypeConfiguration<Verification>
{
    public void Configure(EntityTypeBuilder<Verification> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");

        builder.Property(v => v.Identifier)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(v => v.Value)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(v => v.ExpiresAt)
            .IsRequired();

        builder.HasIndex(v => v.Identifier)
            .HasDatabaseName("IX_Verifications_Identifier");

        builder.HasIndex(v => new { v.Identifier, v.Value })
            .HasDatabaseName("IX_Verifications_Identifier_Value");
    }
}
