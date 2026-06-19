using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Domain.Entities;

/// <summary>A tournament, competition, or real-world event that markets and matches can be linked to.</summary>
public class Event : BaseEntity
{
    /// <summary>Gets the display title of the event.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Gets an optional description of the event.</summary>
    public string? Description { get; private set; }

    /// <summary>Gets the URL-friendly slug for the event.</summary>
    public string? Slug { get; private set; }

    /// <summary>Gets the scheduled start time of the event.</summary>
    public DateTimeOffset? StartTime { get; private set; }

    /// <summary>Gets the scheduled end time of the event.</summary>
    public DateTimeOffset? EndTime { get; private set; }

    /// <summary>Gets the optional category ID this event belongs to.</summary>
    public Guid? CategoryId { get; private set; }

    /// <summary>Gets the URL of the event's main image.</summary>
    public string? ImageUrl { get; private set; }

    /// <summary>Gets the URL of the event's banner image.</summary>
    public string? BannerUrl { get; private set; }

    /// <summary>Gets the current lifecycle status of the event.</summary>
    public EventStatus Status { get; private set; } = EventStatus.Upcoming;

    /// <summary>Optional rich metadata for tournaments/competitions.</summary>
    public string? HostCountry { get; private set; }
    public string? Venue { get; private set; }
    public decimal? PrizePool { get; private set; }

    /// <summary>Gets the category this event belongs to.</summary>
    public Category? Category { get; private set; }

    /// <summary>Gets the prediction markets associated with this event.</summary>
    public ICollection<Market> Markets { get; private set; } = new List<Market>();

    /// <summary>Gets the sports matches that are part of this event.</summary>
    public ICollection<Match> Matches { get; private set; } = new List<Match>();

    /// <summary>Creates a new event.</summary>
    public static Event Create(
        string title,
        string? description = null,
        string? slug = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        Guid? categoryId = null,
        string? imageUrl = null,
        string? bannerUrl = null)
    {
        return new Event
        {
            Title = title,
            Description = description,
            Slug = slug,
            StartTime = startTime,
            EndTime = endTime,
            CategoryId = categoryId,
            ImageUrl = imageUrl,
            BannerUrl = bannerUrl
        };
    }

    /// <summary>Updates the lifecycle status of the event.</summary>
    /// <param name="status">The new status to apply.</param>
    public void UpdateStatus(EventStatus status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
