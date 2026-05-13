using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Features.Markets.DTOs;

public record MarketDto(
    Guid Id,
    string Title,
    string Description,
    DateTimeOffset ResolutionDeadline,
    MarketStatus Status,
    MarketScope Scope,
    Guid CreatorId,
    Guid? GroupId,
    DateTimeOffset CreatedAt);
