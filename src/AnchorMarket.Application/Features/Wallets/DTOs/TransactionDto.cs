using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Features.Wallets.DTOs;

public record TransactionDto(
    Guid Id,
    decimal Amount,
    TransactionType Type,
    string? Description,
    Guid? PositionId,
    DateTimeOffset CreatedAt);
