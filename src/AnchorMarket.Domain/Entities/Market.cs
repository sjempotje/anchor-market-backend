using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Domain.Entities;

/// <summary>A prediction market where users can place positions on one or more outcomes.</summary>
public class Market : BaseEntity
{
    /// <summary>Gets the display title of the market.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Gets the description of what the market resolves on.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Gets the deadline by which the market must be resolved.</summary>
    public DateTimeOffset ResolutionDeadline { get; private set; }

    /// <summary>Gets the current status of the market.</summary>
    public MarketStatus Status { get; private set; } = MarketStatus.Open;

    /// <summary>Gets the visibility scope of the market.</summary>
    public MarketScope Scope { get; private set; } = MarketScope.Public;

    /// <summary>Gets the market type (e.g. binary, multi-choice, moneyline).</summary>
    public MarketType MarketType { get; private set; } = MarketType.Binary;

    /// <summary>The user who created this market.</summary>
    public Guid CreatorId { get; private set; }

    /// <summary>Populated only when Scope == Group.</summary>
    public Guid? GroupId { get; private set; }

    /// <summary>Category for discovery and filtering.</summary>
    public Guid? CategoryId { get; private set; }

    /// <summary>Gets the URL of the market's main image.</summary>
    public string? ImageUrl { get; private set; }

    /// <summary>Gets the URL of the market's banner image.</summary>
    public string? BannerUrl { get; private set; }

    /// <summary>Gets the URL of the market's thumbnail image.</summary>
    public string? Thumbnail { get; private set; }

    /// <summary>Gets the URL-friendly slug for the market.</summary>
    public string? Slug { get; private set; }

    /// <summary>Gets searchable keywords associated with this market.</summary>
    public string? Keywords { get; private set; }

    /// <summary>Gets a value indicating whether this market is featured on the homepage.</summary>
    public bool Featured { get; private set; }

    /// <summary>Gets the algorithmic trending score used for ranking.</summary>
    public decimal TrendingScore { get; private set; }

    /// <summary>Total amount bet on all outcomes.</summary>
    public decimal TotalBetAmount { get; private set; }

    /// <summary>Total number of bets placed.</summary>
    public int BetCount { get; private set; }

    /// <summary>Gets a citation or URL describing the resolution source.</summary>
    public string? ResolutionSource { get; private set; }

    /// <summary>Gets notes written by the resolver explaining the resolution decision.</summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>Gets the group this market belongs to, if group-scoped.</summary>
    public Group? Group { get; private set; }

    /// <summary>Gets the category this market is classified under.</summary>
    public Category? Category { get; private set; }

    /// <summary>Gets the resolution record, populated once the market is resolved.</summary>
    public MarketResolution? Resolution { get; private set; }

    /// <summary>Gets the tradeable outcomes of this market.</summary>
    public ICollection<Outcome> Outcomes { get; private set; } = new List<Outcome>();
    
    /// <summary>Creates a new prediction market with the specified outcomes.</summary>
    public static Market Create(string title, string description, DateTimeOffset resolutionDeadline,
        MarketScope scope, Guid creatorId, Guid? groupId, IReadOnlyList<string> outcomeTitles,
        MarketType marketType = MarketType.Binary, Guid? categoryId = null,
        string? imageUrl = null, string? slug = null)
    {
        var market = new Market
        {
            Title = title,
            Description = description,
            ResolutionDeadline = resolutionDeadline,
            Scope = scope,
            CreatorId = creatorId,
            GroupId = groupId,
            MarketType = marketType,
            CategoryId = categoryId,
            ImageUrl = imageUrl,
            Slug = slug
        };

        foreach (var outcomeTitle in outcomeTitles)
        {
            market.Outcomes.Add(Outcome.Create(outcomeTitle));
        }

        return market;
    }

    /// <summary>Updates the market's core fields.</summary>
    public void Update(string title, string description, DateTimeOffset resolutionDeadline)
    {
        Title = title;
        Description = description;
        ResolutionDeadline = resolutionDeadline;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Updates the market's image URLs.</summary>
    public void SetImages(string? imageUrl, string? bannerUrl, string? thumbnail)
    {
        ImageUrl = imageUrl;
        BannerUrl = bannerUrl;
        Thumbnail = thumbnail;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Sets the resolution source and notes for the market.</summary>
    public void SetResolutionSource(string? source, string? notes)
    {
        ResolutionSource = source;
        ResolutionNotes = notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Updates the total bet amount and count for the market.</summary>
    public void UpdateStats(decimal totalBetAmount, int betCount)
    {
        TotalBetAmount = totalBetAmount;
        BetCount = betCount;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Sets the featured flag and trending score for the market.</summary>
    public void SetFeatured(bool featured, decimal trendingScore = 0)
    {
        Featured = featured;
        TrendingScore = trendingScore;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Transitions the market status to Cancelled.</summary>
    public void MarkAsCancelled()
    {
        Status = MarketStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Transitions the market status to Resolved.</summary>
    public void MarkAsResolved()
    {
        Status = MarketStatus.Resolved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
