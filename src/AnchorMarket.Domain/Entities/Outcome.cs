namespace AnchorMarket.Domain.Entities;

public class Outcome : BaseEntity
{
    public Guid MarketId { get; private set; }
    public string Title { get; private set; } = string.Empty;

    public Market Market { get; private set; } = null!;
    public ICollection<Position> Positions { get; private set; } = new List<Position>();
}
