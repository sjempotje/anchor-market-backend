namespace AnchorMarket.Application.Features.Positions.DTOs;

/// <summary>Data transfer object for a position.</summary>
public record PositionDto(
    Guid Id,
    Guid UserId,
    Guid OutcomeId,
    decimal Amount,
    decimal Shares,
    DateTimeOffset CreatedAt);
