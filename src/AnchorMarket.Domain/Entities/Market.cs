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

    public static Market Create(string title, string description, DateTimeOffset resolutionDeadline,
        MarketScope scope, Guid creatorId, Guid? groupId, IReadOnlyList<string> outcomeTitles)
    {
        var market = new Market
        {
            Title = title,
            Description = description,
            ResolutionDeadline = resolutionDeadline,
            Scope = scope,
            CreatorId = creatorId,
            GroupId = groupId
        };

        foreach (var outcomeTitle in outcomeTitles)
        {
            market.Outcomes.Add(Outcome.Create(outcomeTitle));
        }

        return market;
    }

    public void Update(string title, string description, DateTimeOffset resolutionDeadline)
    {
        Title = title;
        Description = description;
        ResolutionDeadline = resolutionDeadline;
    }

    public void MarkAsCancelled()
    {
        Status = MarketStatus.Cancelled;
    }

    public void MarkAsResolved()
    {
        Status = MarketStatus.Resolved;
    }
}
