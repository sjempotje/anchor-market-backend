namespace AnchorMarket.Domain.Entities;

/// <summary>A sports league or competition series that organises matches within a sport.</summary>
public class League : BaseEntity
{
    /// <summary>Gets the display name of the league.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the URL-friendly slug for the league.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Gets the URL of the league's logo image.</summary>
    public string? LogoUrl { get; private set; }

    /// <summary>Gets the country this league is based in.</summary>
    public string? Country { get; private set; }

    /// <summary>Gets the ID of the sport this league belongs to.</summary>
    public Guid SportId { get; private set; }

    /// <summary>Gets the sport this league belongs to.</summary>
    public Sport Sport { get; private set; } = null!;

    /// <summary>Gets the matches played in this league.</summary>
    public ICollection<Match> Matches { get; private set; } = new List<Match>();

    /// <summary>Creates a new league within the specified sport.</summary>
    /// <param name="name">The league name.</param>
    /// <param name="slug">The URL-friendly slug.</param>
    /// <param name="sportId">The ID of the associated sport.</param>
    /// <param name="logoUrl">Optional logo URL.</param>
    /// <param name="country">Optional country of origin.</param>
    /// <returns>A new <see cref="League"/> instance.</returns>
    public static League Create(string name, string slug, Guid sportId, string? logoUrl = null, string? country = null)
    {
        return new League { Name = name, Slug = slug, SportId = sportId, LogoUrl = logoUrl, Country = country };
    }
}
