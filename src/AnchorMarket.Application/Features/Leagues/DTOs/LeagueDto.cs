namespace AnchorMarket.Application.Features.Leagues.DTOs;

/// <summary>Data transfer object for a league.</summary>
public record LeagueDto(
    Guid Id,
    string Name,
    string Slug,
    string? LogoUrl,
    string? Country,
    Guid SportId,
    DateTimeOffset CreatedAt);
