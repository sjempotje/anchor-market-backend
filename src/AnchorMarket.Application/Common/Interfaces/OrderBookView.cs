namespace AnchorMarket.Application.Common.Interfaces;

/// <summary>A single aggregated price level in the order book.</summary>
/// <param name="Price">The price level.</param>
/// <param name="Quantity">The total resting quantity at that price.</param>
public record OrderBookLevel(decimal Price, decimal Quantity);

/// <summary>The latest traded price recorded for an outcome.</summary>
/// <param name="Price">The latest traded price.</param>
/// <param name="Volume">The shares exchanged in the originating trade.</param>
/// <param name="Timestamp">When the price was established.</param>
public record LatestPrice(decimal Price, decimal Volume, DateTimeOffset Timestamp);

/// <summary>A read-only view of an outcome's live order book.</summary>
/// <param name="Bids">Bid levels, best (highest) first.</param>
/// <param name="Asks">Ask levels, best (lowest) first.</param>
public record OrderBookView(IReadOnlyList<OrderBookLevel> Bids, IReadOnlyList<OrderBookLevel> Asks)
{
    /// <summary>An empty order book.</summary>
    public static readonly OrderBookView Empty = new([], []);

    /// <summary>The best (highest) bid price, or null when there are no bids.</summary>
    public decimal? BestBid => Bids.Count > 0 ? Bids[0].Price : null;

    /// <summary>The best (lowest) ask price, or null when there are no asks.</summary>
    public decimal? BestAsk => Asks.Count > 0 ? Asks[0].Price : null;

    /// <summary>Whether the book has any resting levels.</summary>
    public bool HasLevels => Bids.Count > 0 || Asks.Count > 0;
}
