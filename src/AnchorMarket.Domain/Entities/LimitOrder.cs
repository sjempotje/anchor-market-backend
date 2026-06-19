using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Domain.Entities;

/// <summary>A limit order placed by a user to buy or sell shares on a specific outcome.</summary>
public class LimitOrder : BaseEntity
{
    public Guid MarketId { get; private set; }
    public Guid OutcomeId { get; private set; }
    public Guid UserId { get; private set; }
    
    public OrderSide Side { get; private set; }
    public decimal Price { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal FilledQuantity { get; private set; }
    public decimal TotalCost { get; private set; }
    public OrderType Type { get; private set; } = OrderType.Limit;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public DateTimeOffset? ExpiresAt { get; private set; }

    public Market Market { get; private set; } = null!;
    public Outcome Outcome { get; private set; } = null!;

    public ICollection<TradeExecution> TradeExecutions { get; private set; } = new List<TradeExecution>();

    /// <summary>Creates a buy limit order for the specified outcome.</summary>
    /// <param name="marketId">The target market.</param>
    /// <param name="outcomeId">The target outcome.</param>
    /// <param name="userId">The placing user.</param>
    /// <param name="price">The maximum price per share.</param>
    /// <param name="quantity">The number of shares to buy.</param>
    /// <param name="expiresAt">Optional expiry time for the order.</param>
    /// <returns>A new buy <see cref="LimitOrder"/>.</returns>
    public static LimitOrder CreateBuy(
        Guid marketId,
        Guid outcomeId,
        Guid userId,
        decimal price,
        decimal quantity,
        DateTimeOffset? expiresAt = null)
    {
        var order = new LimitOrder
        {
            MarketId = marketId,
            OutcomeId = outcomeId,
            UserId = userId,
            Side = OrderSide.Buy,
            Price = price,
            Quantity = quantity,
            FilledQuantity = 0,
            TotalCost = 0,
            Type = OrderType.Limit,
            Status = OrderStatus.Pending,
            ExpiresAt = expiresAt
        };

        return order;
    }

    /// <summary>Creates a sell limit order for the specified outcome.</summary>
    /// <param name="marketId">The target market.</param>
    /// <param name="outcomeId">The target outcome.</param>
    /// <param name="userId">The placing user.</param>
    /// <param name="price">The minimum price per share to accept.</param>
    /// <param name="quantity">The number of shares to sell.</param>
    /// <param name="expiresAt">Optional expiry time for the order.</param>
    /// <returns>A new sell <see cref="LimitOrder"/>.</returns>
    public static LimitOrder CreateSell(
        Guid marketId,
        Guid outcomeId,
        Guid userId,
        decimal price,
        decimal quantity,
        DateTimeOffset? expiresAt = null)
    {
        var order = new LimitOrder
        {
            MarketId = marketId,
            OutcomeId = outcomeId,
            UserId = userId,
            Side = OrderSide.Sell,
            Price = price,
            Quantity = quantity,
            FilledQuantity = 0,
            TotalCost = 0,
            Type = OrderType.Limit,
            Status = OrderStatus.Pending,
            ExpiresAt = expiresAt
        };

        return order;
    }

    /// <summary>Attempts to fill a portion of this order; returns false if already filled or the fill amount is invalid.</summary>
    /// <param name="sharesToFill">Number of shares to fill.</param>
    /// <param name="fillPrice">Price per share at which the fill occurs.</param>
    /// <returns><c>true</c> if the fill was successful; otherwise <c>false</c>.</returns>
    public bool TryFill(decimal sharesToFill, decimal fillPrice)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.PartiallyFilled)
            return false;

        var remainingQuantity = Quantity - FilledQuantity;
        if (sharesToFill <= 0 || sharesToFill > remainingQuantity)
            return false;

        FilledQuantity += sharesToFill;
        TotalCost = FilledQuantity * Price;

        if (FilledQuantity >= Quantity)
        {
            Status = OrderStatus.Filled;
        }
        else
        {
            Status = OrderStatus.PartiallyFilled;
        }

        return true;
    }

    /// <summary>Cancels the order; throws if the order is already fully filled.</summary>
    public void Cancel()
    {
        if (Status == OrderStatus.Filled)
            throw new InvalidOperationException("Cannot cancel a fully filled order.");

        Status = OrderStatus.Canceled;
    }

    /// <summary>Returns true if the order has an expiry time that has passed.</summary>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTimeOffset.UtcNow > ExpiresAt.Value;
    }

    /// <summary>Marks the order as expired; no-op if the order is already fully filled.</summary>
    public void MarkExpired()
    {
        if (Status == OrderStatus.Filled)
            return;

        Status = OrderStatus.Expired;
    }
}

/// <summary>A recorded trade execution from matched orders.</summary>
public class TradeExecution : BaseEntity
{
    public Guid LimitOrderId { get; private set; }
    public Guid MarketId { get; private set; }
    public Guid OutcomeId { get; private set; }
    
    public Guid BuyerOrderId { get; private set; }
    public Guid SellerOrderId { get; private set; }
    public Guid InitiatorUserId { get; private set; }
    
    public decimal Shares { get; private set; }
    public decimal ExecutedPrice { get; private set; }
    public decimal TotalValue { get; private set; }

    public LimitOrder LimitOrder { get; private set; } = null!;
    public Market Market { get; private set; } = null!;
    public Outcome Outcome { get; private set; } = null!;

    /// <summary>Creates a new trade execution record.</summary>
    public static TradeExecution Create(
        Guid limitOrderId,
        Guid marketId,
        Guid outcomeId,
        Guid buyerOrderId,
        Guid sellerOrderId,
        Guid initiatorUserId,
        decimal shares,
        decimal executedPrice)
    {
        return new TradeExecution
        {
            LimitOrderId = limitOrderId,
            MarketId = marketId,
            OutcomeId = outcomeId,
            BuyerOrderId = buyerOrderId,
            SellerOrderId = sellerOrderId,
            InitiatorUserId = initiatorUserId,
            Shares = shares,
            ExecutedPrice = executedPrice,
            TotalValue = shares * executedPrice
        };
    }
}
