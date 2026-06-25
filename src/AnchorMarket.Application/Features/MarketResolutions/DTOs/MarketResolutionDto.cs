namespace AnchorMarket.Application.Features.MarketResolutions.DTOs;

/// <summary>Data transfer object describing how a market was resolved.</summary>
public record MarketResolutionDto(
    Guid MarketId,
    Guid WinningOutcomeId,
    Guid ResolvedById,
    DateTimeOffset ResolvedAt);
