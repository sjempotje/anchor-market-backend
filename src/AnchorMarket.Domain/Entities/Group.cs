namespace AnchorMarket.Domain.Entities;

public class Group : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid OwnerId { get; private set; }

    public ICollection<GroupMembership> Memberships { get; private set; } = new List<GroupMembership>();
    public ICollection<Market> Markets { get; private set; } = new List<Market>();

    public static Group Create(string name, string? description, Guid ownerId)
    {
        return new Group { Name = name, Description = description, OwnerId = ownerId };
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
