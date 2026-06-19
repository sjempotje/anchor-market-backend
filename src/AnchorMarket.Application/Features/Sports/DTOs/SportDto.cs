using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Features.Sports.DTOs;

/// <summary>Data transfer object for a sport.</summary>
public record SportDto(
    Guid Id,
    string Name,
    string Slug,
    string? Icon,
    SportType Type,
    DateTimeOffset CreatedAt);
