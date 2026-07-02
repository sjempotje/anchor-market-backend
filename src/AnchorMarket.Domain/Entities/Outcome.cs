namespace AnchorMarket.Domain.Entities;

public class Outcome : BaseEntity
{
    public Guid MarketId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? ShortName { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? Color { get; private set; }
    public string? CountryCode { get; private set; }
    public int SortOrder { get; private set; }

    /// <summary>Total amount bet on this outcome.</summary>
    public decimal TotalBetAmount { get; private set; }

    public Market Market { get; private set; } = null!;
    public ICollection<Position> Positions { get; private set; } = new List<Position>();

    public static Outcome Create(string title, string? shortName = null, string? imageUrl = null,
        string? color = null, string? countryCode = null, int sortOrder = 0)
    {
        return new Outcome
        {
            Title = title,
            ShortName = shortName,
            ImageUrl = imageUrl,
            Color = color,
            CountryCode = countryCode,
            SortOrder = sortOrder
        };
    }

    /// <summary>Updates total bet amount for this outcome.</summary>
    public void UpdateStats(decimal totalBetAmount)
    {
        TotalBetAmount = totalBetAmount;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
