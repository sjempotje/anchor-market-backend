namespace AnchorMarket.Domain.Entities;

/// <summary>Records a user's membership in a prediction market group.</summary>
public class GroupMembership : BaseEntity
{
    /// <summary>Gets the ID of the member user.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the ID of the group.</summary>
    public Guid GroupId { get; private set; }

    /// <summary>Gets the UTC timestamp when the user joined the group.</summary>
    public DateTimeOffset JoinedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets the group this membership belongs to.</summary>
    public Group Group { get; private set; } = null!;

    /// <summary>Creates a new group membership for the given user.</summary>
    /// <param name="userId">The joining user's ID.</param>
    /// <param name="groupId">The target group's ID.</param>
    /// <returns>A new <see cref="GroupMembership"/> instance.</returns>
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
