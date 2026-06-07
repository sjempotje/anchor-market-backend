namespace AnchorMarket.Domain.Entities;

public class GroupMembership : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid GroupId { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Group Group { get; private set; } = null!;

    public static GroupMembership Create(Guid userId, Guid groupId)
    {
        return new GroupMembership
        {
            UserId = userId,
            GroupId = groupId,
            JoinedAt = DateTimeOffset.UtcNow
        };
    }
}
