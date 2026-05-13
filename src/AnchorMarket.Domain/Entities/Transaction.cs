using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Domain.Entities;

/// <summary>A record of a wallet debit or credit</summary>
public class Transaction : BaseEntity
{
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public string? Description { get; private set; }

    /// <summary>Set when the transaction was triggered by placing a bet.</summary>
    public Guid? PositionId { get; private set; }

    public Wallet Wallet { get; private set; } = null!;
}
