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

    /// <summary>The event this market belongs to (e.g. FIFA World Cup).</summary>
    public Guid? EventId { get; private set; }

    /// <summary>Category for discovery and filtering.</summary>
    public Guid? CategoryId { get; private set; }

    /// <summary>The sports match this market is based on.</summary>
    public Guid? MatchId { get; private set; }

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

    /// <summary>Updated by background jobs.</summary>
    public decimal Volume24h { get; private set; }
    /// <summary>Updated by background jobs.</summary>
    public decimal Volume7d { get; private set; }
    /// <summary>Updated by background jobs.</summary>
    public decimal VolumeAllTime { get; private set; }
    /// <summary>Updated by background jobs.</summary>
    public decimal OpenInterest { get; private set; }
    /// <summary>Updated by background jobs.</summary>
    public decimal Liquidity { get; private set; }
    /// <summary>Updated by background jobs.</summary>
    public int TradesCount { get; private set; }

    /// <summary>Gets a citation or URL describing the resolution source.</summary>
    public string? ResolutionSource { get; private set; }

    /// <summary>Gets notes written by the resolver explaining the resolution decision.</summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>Gets the group this market belongs to, if group-scoped.</summary>
    public Group? Group { get; private set; }

    /// <summary>Gets the event this market is associated with.</summary>
    public Event? Event { get; private set; }

    /// <summary>Gets the category this market is classified under.</summary>
    public Category? Category { get; private set; }

    /// <summary>Gets the sports match this market is based on.</summary>
    public Match? Match { get; private set; }

    /// <summary>Gets the resolution record, populated once the market is resolved.</summary>
    public MarketResolution? Resolution { get; private set; }

    /// <summary>Gets the tradeable outcomes of this market.</summary>
    public ICollection<Outcome> Outcomes { get; private set; } = new List<Outcome>();

    /// <summary>Gets the comments posted on this market.</summary>
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

    /// <summary>Gets the users who have favorited this market.</summary>
    public ICollection<FavoriteMarket> FavoritedBy { get; private set; } = new List<FavoriteMarket>();

    /// <summary>Creates a new prediction market with the specified outcomes.</summary>
    public static Market Create(string title, string description, DateTimeOffset resolutionDeadline,
        MarketScope scope, Guid creatorId, Guid? groupId, IReadOnlyList<string> outcomeTitles,
        MarketType marketType = MarketType.Binary, Guid? eventId = null, Guid? categoryId = null,
        Guid? matchId = null, string? imageUrl = null, string? slug = null)
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
            EventId = eventId,
            CategoryId = categoryId,
            MatchId = matchId,
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

    /// <summary>Updates the aggregated trading statistics for the market.</summary>
    public void UpdateStats(decimal volume24h, decimal volume7d, decimal volumeAllTime,
        decimal openInterest, decimal liquidity, int tradesCount)
    {
        Volume24h = volume24h;
        Volume7d = volume7d;
        VolumeAllTime = volumeAllTime;
        OpenInterest = openInterest;
        Liquidity = liquidity;
        TradesCount = tradesCount;
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
