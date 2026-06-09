using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.AccountId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.ProviderId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.AccessToken)
            .HasMaxLength(1024);

        builder.Property(a => a.RefreshToken)
            .HasMaxLength(1024);

        builder.Property(a => a.AccessTokenExpiresAt);

        builder.Property(a => a.RefreshTokenExpiresAt);

        builder.Property(a => a.Scope)
            .HasMaxLength(500);

        builder.Property(a => a.IdToken)
            .HasMaxLength(2048);

        builder.Property(a => a.Password)
            .HasMaxLength(256);

        builder.HasIndex(a => new { a.ProviderId, a.AccountId })
            .IsUnique()
            .HasDatabaseName("IX_Accounts_ProviderId_AccountId");

        builder.HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
