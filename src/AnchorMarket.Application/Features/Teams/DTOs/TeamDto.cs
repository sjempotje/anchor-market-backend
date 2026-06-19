namespace AnchorMarket.Application.Features.Teams.DTOs;

/// <summary>Data transfer object for a team.</summary>
public record TeamDto(
    Guid Id,
    string Name,
    string ShortName,
    string Slug,
    string? LogoUrl,
    string? Country,
    string? CountryCode,
    Guid SportId,
    DateTimeOffset CreatedAt);
