namespace AnchorMarket.Domain.Entities;

/// <summary>A team that competes in matches within a sport.</summary>
public class Team : BaseEntity
{
    /// <summary>Gets the full display name of the team.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the abbreviated name of the team.</summary>
    public string ShortName { get; private set; } = string.Empty;

    /// <summary>Gets the URL-friendly slug for the team.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Gets the URL to the team's logo image.</summary>
    public string? LogoUrl { get; private set; }

    /// <summary>Gets the country the team represents.</summary>
    public string? Country { get; private set; }

    /// <summary>Gets the two-letter country code.</summary>
    public string? CountryCode { get; private set; }

    /// <summary>Gets the ID of the associated sport.</summary>
    public Guid SportId { get; private set; }

    /// <summary>Gets the associated sport.</summary>
    public Sport Sport { get; private set; } = null!;

    /// <summary>Gets the users who have favorited this team.</summary>
    public ICollection<FavoriteTeam> FavoritedBy { get; private set; } = new List<FavoriteTeam>();

    /// <summary>Creates a new team.</summary>
    /// <param name="name">The full display name.</param>
    /// <param name="shortName">The abbreviated name.</param>
    /// <param name="slug">The URL-friendly slug.</param>
    /// <param name="sportId">The ID of the associated sport.</param>
    /// <param name="logoUrl">An optional logo image URL.</param>
    /// <param name="country">An optional country name.</param>
    /// <param name="countryCode">An optional two-letter country code.</param>
    /// <returns>A new <see cref="Team"/> instance.</returns>
    public static Team Create(string name, string shortName, string slug, Guid sportId,
        string? logoUrl = null, string? country = null, string? countryCode = null)
    {
        return new Team
        {
            Name = name,
            ShortName = shortName,
            Slug = slug,
            SportId = sportId,
            LogoUrl = logoUrl,
            Country = country,
            CountryCode = countryCode
        };
    }
}
