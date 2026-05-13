using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

public class OutcomeConfiguration : IEntityTypeConfiguration<Outcome>
{
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
