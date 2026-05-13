namespace AnchorMarket.Application.Features.Positions.DTOs;

public record PositionDto(
    Guid Id,
    Guid UserId,
    Guid OutcomeId,
    decimal Amount,
    decimal Shares,
    DateTimeOffset CreatedAt);
