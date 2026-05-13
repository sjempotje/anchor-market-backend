using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Domain.Entities;

public class Market : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset ResolutionDeadline { get; private set; }
    public MarketStatus Status { get; private set; } = MarketStatus.Open;
    public MarketScope Scope { get; private set; } = MarketScope.Public;

    /// <summary>The user who created this market.</summary>
    public Guid CreatorId { get; private set; }

    /// <summary>Populated only when Scope == Group.</summary>
    public Guid? GroupId { get; private set; }

    public Group? Group { get; private set; }
    public MarketResolution? Resolution { get; private set; }
    public ICollection<Outcome> Outcomes { get; private set; } = new List<Outcome>();
}
