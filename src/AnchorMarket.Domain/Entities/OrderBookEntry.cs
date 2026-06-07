namespace AnchorMarket.Domain.Entities;

/// <summary>
/// Aggregated order book level showing total quantity available at a specific price.
/// This entity stores the snapshot of order book state for persistence and historical analysis.
/// </summary>
public class OrderBookEntry : BaseEntity
{
    public Guid MarketId { get; private set; }
    public Guid OutcomeId { get; private set; }
    
    /// <summary>The price level for this order book entry.</summary>
    public decimal Price { get; private set; }
    
    /// <summary>Total shares available at this price level (aggregated from multiple orders).</summary>
    public decimal TotalQuantity { get; private set; }
    
    /// <summary>Number of active orders at this price level.</summary>
    public int OrderCount { get; private set; }
    
    /// <summary>Whether this is a bid (buy) or ask (sell) side.</summary>
    public OrderBookSide Side { get; private set; }

    /// <summary>The market this order book entry belongs to.</summary>
    public Market Market { get; private set; } = null!;

    /// <summary>The outcome this order book entry is for.</summary>
    public Outcome Outcome { get; private set; } = null!;

    /// <summary>Creates or updates an order book level entry.</summary>
    public static OrderBookEntry CreateLevel(
        Guid marketId,
        Guid outcomeId,
        decimal price,
        OrderBookSide side)
    {
        return new OrderBookEntry
        {
            MarketId = marketId,
            OutcomeId = outcomeId,
            Price = price,
            Side = side,
            TotalQuantity = 0,
            OrderCount = 0
        };
    }

    /// <summary>
    /// Adds quantity to this order book level.
    /// </summary>
    public void AddQuantity(decimal quantity, bool isNewOrder = false)
    {
        TotalQuantity += quantity;
        if (isNewOrder)
            OrderCount++;
    }

    /// <summary>
    /// Removes quantity from this order book level.
    /// Returns true if the level still has remaining quantity.
    /// </summary>
    public bool RemoveQuantity(decimal quantity, bool isOrderCanceled = false)
    {
        if (quantity <= 0) return TotalQuantity > 0;

        TotalQuantity = Math.Max(0, TotalQuantity - quantity);
        
        if (isOrderCanceled && OrderCount > 0)
            OrderCount--;

        return TotalQuantity > 0;
    }

    /// <summary>Clears all quantity from this level.</summary>
    public void Clear()
    {
        TotalQuantity = 0;
        OrderCount = 0;
    }

    /// <summary>Checks if this level is empty.</summary>
    public bool IsEmpty => TotalQuantity <= 0;
}

/// <summary>
/// Represents which side of the order book an entry belongs to.
/// </summary>
public enum OrderBookSide
{
    Bid,  // Buy orders
    Ask   // Sell orders
}
