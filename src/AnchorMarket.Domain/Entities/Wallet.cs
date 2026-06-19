namespace AnchorMarket.Domain.Entities;

/// <summary>Each user's virtual currency balance.</summary>
public class Wallet : BaseEntity
{
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }

    /// <summary>Optimistic concurrency token incremented on every balance change, prevents concurrent double-spend.</summary>
    public uint Version { get; private set; }

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
        Version++;
    }

    public void Credit(decimal amount)
    {
        Balance += amount;
        Version++;
    }
}
