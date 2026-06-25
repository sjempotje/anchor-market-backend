namespace AnchorMarket.Application.Features.Markets.DTOs;

/// <summary>Data transfer object for a market outcome.</summary>
public record OutcomeDto(
    Guid Id,
    Guid MarketId,
    string Title,
    string? ShortName,
    string? ImageUrl,
    string? Color,
    string? CountryCode,
    int SortOrder,
    decimal Volume,
    decimal OpenInterest);
