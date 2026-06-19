using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Features.Wallets.DTOs;

/// <summary>Data transfer object for a wallet transaction.</summary>
public record TransactionDto(
    Guid Id,
    decimal Amount,
    TransactionType Type,
    string? Description,
    Guid? PositionId,
    DateTimeOffset CreatedAt);
