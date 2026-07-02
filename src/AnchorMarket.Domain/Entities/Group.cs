namespace AnchorMarket.Domain.Entities;

/// <summary>A group of users that can create and resolve their own prediction markets.</summary>
public class Group : BaseEntity
{
    /// <summary>Gets the display name of the group.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets an optional description of the group.</summary>
    public string? Description { get; private set; }

    /// <summary>Gets the ID of the user who owns and administers the group.</summary>
    public Guid OwnerId { get; private set; }

    /// <summary>Gets whether this group is private (requires join code).</summary>
    public bool IsPrivate { get; private set; }

    /// <summary>Gets the join code for private groups. Users must provide this code to join.</summary>
    public string? JoinCode { get; private set; }

    /// <summary>Gets the membership records for this group.</summary>
    public ICollection<GroupMembership> Memberships { get; private set; } = new List<GroupMembership>();

    /// <summary>Gets the group-scoped prediction markets.</summary>
    public ICollection<Market> Markets { get; private set; } = new List<Market>();

    /// <summary>Creates a new group with the specified owner.</summary>
    /// <param name="name">The group name.</param>
    /// <param name="description">An optional group description.</param>
    /// <param name="ownerId">The ID of the creating user, who becomes the owner.</param>
    /// <param name="isPrivate">Whether this group requires a join code to join.</param>
    /// <returns>A new <see cref="Group"/> instance.</returns>
    public static Group Create(string name, string? description, Guid ownerId, bool isPrivate = false)
    {
        var group = new Group
        {
            Name = name,
            Description = description,
            OwnerId = ownerId,
            IsPrivate = isPrivate
        };

        if (isPrivate)
        {
            group.JoinCode = GenerateJoinCode();
        }

        return group;
    }

    /// <summary>Updates the name and description of the group.</summary>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>Regenerates the join code for a private group.</summary>
    public void RegenerateJoinCode()
    {
        if (IsPrivate)
        {
            JoinCode = GenerateJoinCode();
        }
    }

    private static string GenerateJoinCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }
}
