namespace AnchorMarket.Application.Common.Interfaces;

/// <summary>Service for managing user wallet balances.</summary>
public interface IWalletService
{
    /// <summary>Debits (locks) funds from a user's wallet.</summary>
    Task DebitBalance(Guid userId, decimal amount);

    /// <summary>Credits (returns) funds to a user's wallet.</summary>
    Task CreditBalance(Guid userId, decimal amount);
}
