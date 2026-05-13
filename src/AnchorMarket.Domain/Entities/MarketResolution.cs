namespace AnchorMarket.Domain.Entities;

/// <summary>The confirmed final outcome of a market. In group markets, resolver must differ from creator.</summary>
public class MarketResolution : BaseEntity
{
    public Guid MarketId { get; private set; }
    public Guid WinningOutcomeId { get; private set; }
    public Guid ResolvedById { get; private set; }
    public DateTimeOffset ResolvedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Market Market { get; private set; } = null!;
    public Outcome WinningOutcome { get; private set; } = null!;
}
