using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="GroupMembership"/> entity.</summary>
public class GroupMembershipConfiguration : IEntityTypeConfiguration<GroupMembership>
{
    /// <summary>Configures the <see cref="GroupMembership"/> entity mappings.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<GroupMembership> builder)
    {
        builder.HasKey(gm => gm.Id);

        builder.HasIndex(gm => new { gm.UserId, gm.GroupId })
            .IsUnique();

        builder.Property(gm => gm.UserId)
            .IsRequired();

        builder.Property(gm => gm.GroupId)
            .IsRequired();
    }
}
