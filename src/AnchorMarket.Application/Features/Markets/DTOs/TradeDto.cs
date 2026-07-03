namespace AnchorMarket.Application.Features.Markets.DTOs;

/// <summary>A single executed trade (bet) against an outcome.</summary>
public record TradeDto(Guid Id, Guid OutcomeId, decimal Price, decimal Shares, DateTimeOffset CreatedAt);
