using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Features.Markets.DTOs;

/// <summary>Data transfer object for a market.</summary>
public record MarketDto(
    Guid Id,
    string Title,
    string Description,
    DateTimeOffset ResolutionDeadline,
    MarketStatus Status,
    MarketScope Scope,
    MarketType MarketType,
    Guid CreatorId,
    Guid? AssignedResolverId,
    Guid? GroupId,
    Guid? CategoryId,
    string? ImageUrl,
    string? BannerUrl,
    string? Thumbnail,
    string? Slug,
    bool Featured,
    decimal TrendingScore,
    decimal TotalBetAmount,
    int BetCount,
    string? ResolutionSource,
    DateTimeOffset CreatedAt);
