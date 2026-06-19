using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Orders.DTOs;
using AnchorMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Orders.Queries;

/// <summary>Query to retrieve the order book for a market outcome.</summary>
public record GetOrderBookQuery(
    Guid MarketId, 
    Guid OutcomeId) : IRequest<OrderBookDto>;

/// <summary>Handles retrieving the order book.</summary>
public class GetOrderBookQueryHandler : IRequestHandler<GetOrderBookQuery, OrderBookDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetOrderBookQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Returns the bid/ask levels for the specified market outcome.</summary>
    public async Task<OrderBookDto> Handle(
        GetOrderBookQuery request, 
        CancellationToken cancellationToken)
    {
        var market = await _dbContext.Markets
            .Include(m => m.Outcomes)
            .FirstOrDefaultAsync(m => m.Id == request.MarketId, cancellationToken);

        if (market is null)
            throw new NotFoundException("Market not found.");

        var outcome = market.Outcomes.FirstOrDefault(o => o.Id == request.OutcomeId);
        if (outcome is null)
            throw new NotFoundException("Outcome not found for this market.");

        var openOrders = await _dbContext.LimitOrders
            .Where(o => o.MarketId == request.MarketId 
                        && o.OutcomeId == request.OutcomeId
                        && (o.Status == Domain.Enums.OrderStatus.Pending 
                            || o.Status == Domain.Enums.OrderStatus.PartiallyFilled))
            .ToListAsync(cancellationToken);

        var bidLevels = openOrders
            .Where(o => o.Side == Domain.Enums.OrderSide.Buy)
            .GroupBy(o => o.Price)
            .Select(g => new OrderBookLevelDto(
                g.Key,
                g.Sum(o => o.Quantity - o.FilledQuantity),
                g.Count()))
            .OrderByDescending(l => l.Price)
            .ToList();

        var askLevels = openOrders
            .Where(o => o.Side == Domain.Enums.OrderSide.Sell)
            .GroupBy(o => o.Price)
            .Select(g => new OrderBookLevelDto(
                g.Key,
                g.Sum(o => o.Quantity - o.FilledQuantity),
                g.Count()))
            .OrderBy(l => l.Price)
            .ToList();

        var bestBid = bidLevels.FirstOrDefault()?.Price;
        var bestAsk = askLevels.FirstOrDefault()?.Price;
        var spread = (bestBid.HasValue && bestAsk.HasValue) ? bestAsk.Value - bestBid.Value : 0m;

        return new OrderBookDto(
            request.MarketId,
            market.Title,
            request.OutcomeId,
            outcome.Title,
            bidLevels,
            askLevels,
            bestBid,
            bestAsk,
            spread,
            DateTimeOffset.UtcNow);
    }
}

/// <summary>Query to retrieve the current market price for an outcome.</summary>
public record GetMarketPriceQuery(Guid MarketId, Guid OutcomeId) : IRequest<MarketPriceDto>;

/// <summary>Handles retrieving the current market price.</summary>
public class GetMarketPriceQueryHandler : IRequestHandler<GetMarketPriceQuery, MarketPriceDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMarketPriceQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Calculates the mid-price from the order book and 24h stats.</summary>
    public async Task<MarketPriceDto> Handle(
        GetMarketPriceQuery request,
        CancellationToken cancellationToken)
    {
        var market = await _dbContext.Markets
            .Include(m => m.Outcomes)
            .FirstOrDefaultAsync(m => m.Id == request.MarketId, cancellationToken);

        if (market is null)
            throw new NotFoundException("Market not found.");

        var outcome = market.Outcomes.FirstOrDefault(o => o.Id == request.OutcomeId);
        if (outcome is null)
            throw new NotFoundException("Outcome not found for this market.");

        var now = DateTimeOffset.UtcNow;
        var twentyFourHoursAgo = now.AddHours(-24);

        var openOrders = await _dbContext.LimitOrders
            .Where(o => o.MarketId == request.MarketId
                        && o.OutcomeId == request.OutcomeId
                        && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.PartiallyFilled))
            .ToListAsync(cancellationToken);

        var bestBid = openOrders
            .Where(o => o.Side == OrderSide.Buy)
            .OrderByDescending(o => o.Price)
            .FirstOrDefault()?.Price;

        var bestAsk = openOrders
            .Where(o => o.Side == OrderSide.Sell)
            .OrderBy(o => o.Price)
            .FirstOrDefault()?.Price;

        var currentPrice = bestBid.HasValue && bestAsk.HasValue
            ? (bestBid.Value + bestAsk.Value) / 2
            : 0.5m;

        var trades = await _dbContext.LimitOrders
            .Include(o => o.TradeExecutions)
            .Where(o => o.MarketId == request.MarketId && o.OutcomeId == request.OutcomeId)
            .SelectMany(o => o.TradeExecutions)
            .ToListAsync(cancellationToken);

        var trades24h = trades.Where(t => t.CreatedAt >= twentyFourHoursAgo).ToList();
        var tradesBefore24h = trades.Where(t => t.CreatedAt < twentyFourHoursAgo).ToList();

        var previousPrice = tradesBefore24h.Count != 0
            ? tradesBefore24h.OrderByDescending(t => t.CreatedAt).First().ExecutedPrice
            : currentPrice;

        var high24h = trades24h.Count != 0 ? trades24h.Max(t => t.ExecutedPrice) : currentPrice;
        var low24h = trades24h.Count != 0 ? trades24h.Min(t => t.ExecutedPrice) : currentPrice;
        var volume24h = trades24h.Sum(t => t.TotalValue);

        var change24h = currentPrice - previousPrice;
        var change24hPercent = previousPrice > 0 ? (change24h / previousPrice) * 100 : 0;

        return new MarketPriceDto(
            request.MarketId,
            market.Title,
            request.OutcomeId,
            outcome.Title,
            currentPrice,
            previousPrice,
            change24h,
            change24hPercent,
            high24h,
            low24h,
            volume24h,
            now);
    }
}
