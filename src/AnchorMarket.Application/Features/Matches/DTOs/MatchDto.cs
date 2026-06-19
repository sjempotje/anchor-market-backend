using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Features.Matches.DTOs;

/// <summary>Data transfer object for a match.</summary>
public record MatchDto(
    Guid Id,
    Guid HomeTeamId,
    Guid AwayTeamId,
    Guid LeagueId,
    Guid? EventId,
    DateTimeOffset StartTime,
    MatchStatus Status,
    DateTimeOffset CreatedAt);

/// <summary>Data transfer object for a match's live state.</summary>
public record MatchStateDto(
    Guid Id,
    Guid MatchId,
    int ScoreHome,
    int ScoreAway,
    string? CurrentPeriod,
    string? Clock,
    string? ExtraInfo,
    DateTimeOffset? UpdatedAt);
