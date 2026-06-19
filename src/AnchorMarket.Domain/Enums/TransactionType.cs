namespace AnchorMarket.Domain.Enums;

/// <summary>Indicates the direction of a wallet transaction.</summary>
public enum TransactionType
{
    /// <summary>Funds leaving the wallet (purchase, withdrawal).</summary>
    Debit,
    /// <summary>Funds entering the wallet (deposit, sale, payout).</summary>
    Credit
}
