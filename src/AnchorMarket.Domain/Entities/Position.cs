namespace AnchorMarket.Domain.Entities;

/// <summary>A user's position on a market outcome (supports both simple betting and complex trading).</summary>
public class Position : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid OutcomeId { get; private set; }
    public Guid MarketId { get; private set; }

    public decimal Amount { get; private set; }
    public decimal Shares { get; private set; }
    public decimal EntryPrice { get; private set; }
    public decimal FairValueAtEntry { get; private set; }
    public decimal CurrentFairValue { get; private set; }
    public decimal? Payout { get; private set; }

    public Outcome Outcome { get; private set; } = null!;

    /// <summary>Creates a new simple bet.</summary>
    public static Position Create(Guid userId, Guid outcomeId, Guid marketId, decimal amount)
    {
        return new Position
        {
            UserId = userId,
            OutcomeId = outcomeId,
            MarketId = marketId,
            Amount = amount,
            Shares = 0,
            EntryPrice = 0,
            FairValueAtEntry = 0,
            CurrentFairValue = 0,
            Payout = null
        };
    }

    /// <summary>Creates a position with entry price tracking (for trading).</summary>
    public static Position Create(
        Guid userId,
        Guid outcomeId,
        decimal amount,
        decimal shares,
        decimal entryPrice,
        decimal fairValueAtEntry)
    {
        return new Position
        {
            UserId = userId,
            OutcomeId = outcomeId,
            Amount = amount,
            Shares = shares,
            EntryPrice = entryPrice,
            FairValueAtEntry = fairValueAtEntry,
            CurrentFairValue = fairValueAtEntry
        };
    }

    /// <summary>Resolves the position with a payout after market resolution.</summary>
    public void Resolve(decimal payoutAmount)
    {
        Payout = payoutAmount;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateFairValue(decimal newFairValue)
    {
        CurrentFairValue = Math.Max(0, Math.Min(1, newFairValue));
    }

    public void UpdatePosition(decimal additionalShares, decimal totalValue, decimal executedPrice)
    {
        if (additionalShares <= 0) return;

        Shares += additionalShares;
        Amount += totalValue;
        var newTotalShares = Shares;
        EntryPrice = (EntryPrice * (newTotalShares - additionalShares) + executedPrice * additionalShares) / newTotalShares;
    }

    public void ReducePosition(decimal sharesToReduce)
    {
        if (sharesToReduce <= 0 || Shares <= 0) return;

        var actualReduce = Math.Min(sharesToReduce, Shares);
        Amount = Amount * (Shares - actualReduce) / Shares;
        Shares -= actualReduce;
    }

    public decimal CalculateUnrealizedPnL()
    {
        if (EntryPrice <= 0) return 0;

        var pnlPerShare = CurrentFairValue - EntryPrice;
        return pnlPerShare * Shares;
    }

    public decimal CalculateReturnOnInvestment()
    {
        if (Amount <= 0) return 0;

        var pnl = CalculateUnrealizedPnL();
        return (pnl / Amount) * 100;
    }
}
