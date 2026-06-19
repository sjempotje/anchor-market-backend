namespace AnchorMarket.Domain.Entities;

/// <summary>A historical price data point for an outcome, capturing price, volume, and liquidity at a point in time.</summary>
public class PriceHistory : BaseEntity
{
    /// <summary>Gets the ID of the associated outcome.</summary>
    public Guid OutcomeId { get; private set; }

    /// <summary>Gets the timestamp when this price snapshot was recorded.</summary>
    public DateTimeOffset Timestamp { get; private set; }

    /// <summary>Gets the price at this point in time.</summary>
    public decimal Price { get; private set; }

    /// <summary>Gets the trading volume at this point in time.</summary>
    public decimal Volume { get; private set; }

    /// <summary>Gets the available liquidity at this point in time.</summary>
    public decimal Liquidity { get; private set; }

    /// <summary>Gets the associated outcome.</summary>
    public Outcome Outcome { get; private set; } = null!;

    /// <summary>Creates a new price history record.</summary>
    /// <param name="outcomeId">The ID of the associated outcome.</param>
    /// <param name="timestamp">The timestamp of the snapshot.</param>
    /// <param name="price">The price at this point.</param>
    /// <param name="volume">The trading volume at this point.</param>
    /// <param name="liquidity">The available liquidity at this point.</param>
    /// <returns>A new <see cref="PriceHistory"/> instance.</returns>
    public static PriceHistory Create(Guid outcomeId, DateTimeOffset timestamp, decimal price,
        decimal volume = 0, decimal liquidity = 0)
    {
        return new PriceHistory
        {
            OutcomeId = outcomeId,
            Timestamp = timestamp,
            Price = price,
            Volume = volume,
            Liquidity = liquidity
        };
    }
}
