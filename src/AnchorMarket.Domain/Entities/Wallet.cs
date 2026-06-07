namespace AnchorMarket.Domain.Entities;

/// <summary>Each user's virtual currency balance.</summary>
public class Wallet : BaseEntity
{
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }

    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

    public static Wallet Create(Guid userId)
    {
        return new Wallet { UserId = userId, Balance = 0m };
    }

    public void Debit(decimal amount)
    {
        if (Balance < amount)
            throw new InvalidOperationException("Insufficient balance.");

        Balance -= amount;
    }

    public void Credit(decimal amount)
    {
        Balance += amount;
    }
}
