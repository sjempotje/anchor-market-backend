namespace AnchorMarket.Domain.Entities;

/// <summary>Each user's virtual currency balance.</summary>
public class Wallet : BaseEntity
{
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }

    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
}
