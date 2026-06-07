namespace AnchorMarket.Domain.Entities;

/// <summary>A user's stake on a specific market outcome.</summary>
public class Position : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid OutcomeId { get; private set; }

    public decimal Amount { get; private set; }
    public decimal Shares { get; private set; }
    public decimal EntryPrice { get; private set; }
    public decimal FairValueAtEntry { get; private set; }
    public decimal CurrentFairValue { get; private set; }

    public Outcome Outcome { get; private set; } = null!;

    /// <summary>Creates a new position with entry price tracking.</summary>
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

    public void UpdateFairValue(decimal newFairValue)
    {
        CurrentFairValue = Math.Max(0, Math.Min(1, newFairValue));
    }

    /// <summary>Updates position when additional shares are purchased (average down/up).</summary>
    public void UpdatePosition(decimal additionalShares, decimal totalValue, decimal executedPrice)
    {
        if (additionalShares <= 0) return;
        
        Shares += additionalShares;
        Amount += totalValue;
        var newTotalShares = Shares;
        EntryPrice = (EntryPrice * (newTotalShares - additionalShares) + executedPrice * additionalShares) / newTotalShares;
    }

    /// <summary>Reduces shares without changing entry price. Amount stays unchanged so cost basis is preserved when partially closing.</summary>
    public void ReducePosition(decimal sharesToReduce)
    {
        if (sharesToReduce <= 0) return;
        
        Shares = Math.Max(0, Shares - sharesToReduce);
    }

    /// <summary>Calculates unrealized P&L based on current vs entry fair value.</summary>
    public decimal CalculateUnrealizedPnL()
    {
        if (EntryPrice <= 0) return 0;
        
        var pnlPerShare = CurrentFairValue - EntryPrice;
        return pnlPerShare * Shares;
    }

    /// <summary>Calculates return on investment percentage.</summary>
    public decimal CalculateReturnOnInvestment()
    {
        if (Amount <= 0) return 0;
        
        var pnl = CalculateUnrealizedPnL();
        return (pnl / Amount) * 100;
    }
}
