namespace AnchorMarket.Domain.Entities;

/// <summary>A private group of users that can create and resolve their own prediction markets.</summary>
public class Group : BaseEntity
{
    /// <summary>Gets the display name of the group.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets an optional description of the group.</summary>
    public string? Description { get; private set; }

    /// <summary>Gets the ID of the user who owns and administers the group.</summary>
    public Guid OwnerId { get; private set; }

    /// <summary>Gets the membership records for this group.</summary>
    public ICollection<GroupMembership> Memberships { get; private set; } = new List<GroupMembership>();

    /// <summary>Gets the group-scoped prediction markets.</summary>
    public ICollection<Market> Markets { get; private set; } = new List<Market>();

    /// <summary>Creates a new group with the specified owner.</summary>
    /// <param name="name">The group name.</param>
    /// <param name="description">An optional group description.</param>
    /// <param name="ownerId">The ID of the creating user, who becomes the owner.</param>
    /// <returns>A new <see cref="Group"/> instance.</returns>
    public static Group Create(string name, string? description, Guid ownerId)
    {
        return new Group { Name = name, Description = description, OwnerId = ownerId };
    }

    /// <summary>Updates the name and description of the group.</summary>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
