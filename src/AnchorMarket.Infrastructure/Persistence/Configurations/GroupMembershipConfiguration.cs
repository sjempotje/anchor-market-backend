using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnchorMarket.Infrastructure.Persistence.Configurations;

public class GroupMembershipConfiguration : IEntityTypeConfiguration<GroupMembership>
{
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
