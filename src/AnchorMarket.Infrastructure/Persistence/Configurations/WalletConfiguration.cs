using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="Wallet"/> entity.</summary>
public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    /// <summary>Configures the <see cref="Wallet"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.UserId)
            .IsUnique();

        builder.Property(w => w.UserId)
            .IsRequired();

        builder.Property(w => w.Balance)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.HasOne<User>()
            .WithOne(u => u.Wallet)
            .HasForeignKey<Wallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
