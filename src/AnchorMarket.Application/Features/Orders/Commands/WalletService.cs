using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Orders.Commands;

public class WalletService : IWalletService
{
    private readonly IApplicationDbContext _dbContext;

    public WalletService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task DebitBalance(Guid userId, decimal amount)
    {
        var wallet = await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet is null)
        {
            wallet = Wallet.Create(userId);
            _dbContext.Wallets.Add(wallet);
        }

        wallet.Debit(amount);

        var transaction = Transaction.CreateDebit(
            wallet.Id, amount, "Limit order deposit");

        _dbContext.Transactions.Add(transaction);
    }

    public async Task CreditBalance(Guid userId, decimal amount)
    {
        var wallet = await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet is null)
        {
            wallet = Wallet.Create(userId);
            _dbContext.Wallets.Add(wallet);
        }

        wallet.Credit(amount);

        var transaction = Transaction.CreateCredit(
            wallet.Id, amount, "Limit order cancellation");

        _dbContext.Transactions.Add(transaction);
    }
}
