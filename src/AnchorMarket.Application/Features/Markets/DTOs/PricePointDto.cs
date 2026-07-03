namespace AnchorMarket.Application.Features.Markets.DTOs;

/// <summary>A single sampled implied-probability price for an outcome.</summary>
public record PricePointDto(decimal Price, decimal Volume, DateTimeOffset Timestamp);
