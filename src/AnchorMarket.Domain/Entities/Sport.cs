using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Domain.Entities;

/// <summary>A sport discipline that groups leagues and teams (e.g. Soccer, Basketball).</summary>
public class Sport : BaseEntity
{
    /// <summary>Gets the display name of the sport.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the URL-friendly slug for the sport.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Gets an optional icon identifier for the sport.</summary>
    public string? Icon { get; private set; }

    /// <summary>Gets the type of sport (e.g. team, individual).</summary>
    public SportType Type { get; private set; }

    /// <summary>Gets the leagues belonging to this sport.</summary>
    public ICollection<League> Leagues { get; private set; } = new List<League>();

    /// <summary>Gets the teams participating in this sport.</summary>
    public ICollection<Team> Teams { get; private set; } = new List<Team>();

    /// <summary>Creates a new sport.</summary>
    /// <param name="name">The display name.</param>
    /// <param name="slug">The URL-friendly slug.</param>
    /// <param name="type">The sport type.</param>
    /// <param name="icon">An optional icon identifier.</param>
    /// <returns>A new <see cref="Sport"/> instance.</returns>
    public static Sport Create(string name, string slug, SportType type, string? icon = null)
    {
        return new Sport { Name = name, Slug = slug, Type = type, Icon = icon };
    }
}
