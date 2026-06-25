namespace AnchorMarket.Domain.Entities;

/// <summary>A point-in-time snapshot of an outcome's order book, captured for historical charting.</summary>
public class OrderBookSnapshot : BaseEntity
{
    /// <summary>Gets the ID of the outcome this snapshot belongs to.</summary>
    public Guid OutcomeId { get; private set; }

    /// <summary>Gets the timestamp the snapshot was captured.</summary>
    public DateTimeOffset Timestamp { get; private set; }

    /// <summary>Gets the bid levels as a JSON array of {Price, Quantity}, best first.</summary>
    public string Bids { get; private set; } = "[]";

    /// <summary>Gets the ask levels as a JSON array of {Price, Quantity}, best first.</summary>
    public string Asks { get; private set; } = "[]";

    /// <summary>Gets the best (highest) bid price, or null if there were no bids.</summary>
    public decimal? BestBid { get; private set; }

    /// <summary>Gets the best (lowest) ask price, or null if there were no asks.</summary>
    public decimal? BestAsk { get; private set; }

    /// <summary>Gets the spread between best ask and best bid, or zero if either side is empty.</summary>
    public decimal Spread { get; private set; }

    /// <summary>Gets the outcome this snapshot belongs to.</summary>
    public Outcome Outcome { get; private set; } = null!;

    /// <summary>Creates a new order book snapshot.</summary>
    /// <param name="outcomeId">The outcome the snapshot belongs to.</param>
    /// <param name="timestamp">When the snapshot was captured.</param>
    /// <param name="bids">Bid levels as a JSON array, best first.</param>
    /// <param name="asks">Ask levels as a JSON array, best first.</param>
    /// <param name="bestBid">The best bid price, if any.</param>
    /// <param name="bestAsk">The best ask price, if any.</param>
    /// <returns>A new <see cref="OrderBookSnapshot"/> instance.</returns>
    public static OrderBookSnapshot Create(
        Guid outcomeId,
        DateTimeOffset timestamp,
        string bids,
        string asks,
        decimal? bestBid,
        decimal? bestAsk)
    {
        return new OrderBookSnapshot
        {
            OutcomeId = outcomeId,
            Timestamp = timestamp,
            Bids = bids,
            Asks = asks,
            BestBid = bestBid,
            BestAsk = bestAsk,
            Spread = bestBid.HasValue && bestAsk.HasValue ? bestAsk.Value - bestBid.Value : 0
        };
    }
}
