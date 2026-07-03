namespace AnchorMarket.Domain.Entities;

/// <summary>A recorded implied-probability price for an outcome at a point in time, sampled on each trade.</summary>
public class OutcomePricePoint : BaseEntity
{
    public Guid OutcomeId { get; private set; }
    public decimal Price { get; private set; }
    public decimal Volume { get; private set; }

    /// <summary>True when this point records the outcome that was actually traded.</summary>
    public bool IsTrade { get; private set; }

    public Outcome Outcome { get; private set; } = null!;

    public static OutcomePricePoint Create(Guid outcomeId, decimal price, decimal volume, bool isTrade = false)
    {
        return new OutcomePricePoint
        {
            OutcomeId = outcomeId,
            Price = price,
            Volume = volume,
            IsTrade = isTrade
        };
    }
}
