using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Features.Events.DTOs;

/// <summary>Data transfer object for an event.</summary>
public record EventDto(
    Guid Id,
    string Title,
    string? Description,
    string? Slug,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    Guid? CategoryId,
    string? ImageUrl,
    string? BannerUrl,
    EventStatus Status,
    string? HostCountry,
    string? Venue,
    decimal? PrizePool,
    DateTimeOffset CreatedAt);
