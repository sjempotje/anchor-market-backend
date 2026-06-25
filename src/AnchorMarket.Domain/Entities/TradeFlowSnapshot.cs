namespace AnchorMarket.Domain.Entities;

/// <summary>
/// A trade execution enriched with the order book depth present at the moment it executed,
/// captured for trade-flow analysis and charting.
/// </summary>
public class TradeFlowSnapshot : BaseEntity
{
    /// <summary>Gets the market the trade occurred on.</summary>
    public Guid MarketId { get; private set; }

    /// <summary>Gets the outcome that was traded.</summary>
    public Guid OutcomeId { get; private set; }

    /// <summary>Gets the timestamp the trade executed.</summary>
    public DateTimeOffset Timestamp { get; private set; }

    /// <summary>Gets the execution price.</summary>
    public decimal ExecutedPrice { get; private set; }

    /// <summary>Gets the number of shares exchanged.</summary>
    public decimal Shares { get; private set; }

    /// <summary>Gets the buyer's order ID.</summary>
    public Guid BuyerOrderId { get; private set; }

    /// <summary>Gets the seller's order ID.</summary>
    public Guid SellerOrderId { get; private set; }

    /// <summary>Gets the total resting bid depth at the moment of the trade.</summary>
    public decimal BidDepthAtTrade { get; private set; }

    /// <summary>Gets the total resting ask depth at the moment of the trade.</summary>
    public decimal AskDepthAtTrade { get; private set; }

    /// <summary>Gets the outcome that was traded.</summary>
    public Outcome Outcome { get; private set; } = null!;

    /// <summary>Creates a new trade flow snapshot.</summary>
    public static TradeFlowSnapshot Create(
        Guid marketId,
        Guid outcomeId,
        DateTimeOffset timestamp,
        decimal executedPrice,
        decimal shares,
        Guid buyerOrderId,
        Guid sellerOrderId,
        decimal bidDepthAtTrade,
        decimal askDepthAtTrade)
    {
        return new TradeFlowSnapshot
        {
            MarketId = marketId,
            OutcomeId = outcomeId,
            Timestamp = timestamp,
            ExecutedPrice = executedPrice,
            Shares = shares,
            BuyerOrderId = buyerOrderId,
            SellerOrderId = sellerOrderId,
            BidDepthAtTrade = bidDepthAtTrade,
            AskDepthAtTrade = askDepthAtTrade
        };
    }
}
