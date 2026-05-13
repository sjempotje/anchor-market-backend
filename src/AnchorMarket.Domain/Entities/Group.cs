namespace AnchorMarket.Domain.Entities;

public class Group : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid OwnerId { get; private set; }

    public ICollection<GroupMembership> Memberships { get; private set; } = new List<GroupMembership>();
    public ICollection<Market> Markets { get; private set; } = new List<Market>();
}
