using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Realtime;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Orders.Commands;

/// <summary>Command to execute the order matching engine for a market.</summary>
public record MatchOrdersCommand(Guid MarketId, Guid? OutcomeId = null) : IRequest<MatchingResult>;

/// <summary>Result of the order matching engine execution.</summary>
public record MatchingResult(
    int TradesExecuted,
    decimal TotalVolume,
    IReadOnlyList<TradeExecution> ExecutedTrades);

/// <summary>Handles the order matching engine.</summary>
public class MatchOrdersCommandHandler : IRequestHandler<MatchOrdersCommand, MatchingResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IWalletService _walletService;
    private readonly IOrderBookCache _orderBookCache;
    private readonly IRealtimePublisher _realtimePublisher;

    public MatchOrdersCommandHandler(
        IApplicationDbContext dbContext,
        IWalletService walletService,
        IOrderBookCache orderBookCache,
        IRealtimePublisher realtimePublisher)
    {
        _dbContext = dbContext;
        _walletService = walletService;
        _orderBookCache = orderBookCache;
        _realtimePublisher = realtimePublisher;
    }

    /// <summary>Captures a fill so its cache and broadcast side effects can be flushed after commit.</summary>
    private readonly record struct Fill(
        Guid MarketId,
        Guid OutcomeId,
        decimal BuyPrice,
        decimal SellPrice,
        decimal ExecutedPrice,
        decimal Shares,
        DateTimeOffset Timestamp);

    /// <summary>Matches buy and sell orders and returns the execution result.</summary>
    public async Task<MatchingResult> Handle(MatchOrdersCommand request, CancellationToken cancellationToken)
    {
        var executedTrades = new List<TradeExecution>();
        var fills = new List<Fill>();
        decimal totalVolume = 0;

        var query = _dbContext.LimitOrders
            .Include(o => o.Market)
            .Include(o => o.Outcome)
            .Where(o => o.MarketId == request.MarketId 
                        && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.PartiallyFilled));

        if (request.OutcomeId.HasValue)
            query = query.Where(o => o.OutcomeId == request.OutcomeId.Value);

        var openOrders = await query.ToListAsync(cancellationToken);

        var buyOrders = openOrders.Where(o => o.Side == OrderSide.Buy)
            .OrderByDescending(o => o.Price)
            .ThenBy(o => o.CreatedAt)
            .ToList();

        var sellOrders = openOrders.Where(o => o.Side == OrderSide.Sell)
            .OrderBy(o => o.Price)
            .ThenBy(o => o.CreatedAt)
            .ToList();

        var buyIndex = 0;
        var sellIndex = 0;

        while (buyIndex < buyOrders.Count && sellIndex < sellOrders.Count)
        {
            var bestBuy = buyOrders[buyIndex];
            var bestSell = sellOrders[sellIndex];

            if (bestBuy.Price >= bestSell.Price)
            {
                var tradeShares = Math.Min(
                    bestBuy.Quantity - bestBuy.FilledQuantity,
                    bestSell.Quantity - bestSell.FilledQuantity);

                var executedPrice = (bestBuy.Price + bestSell.Price) / 2;

                // Resting depth on each side just before this trade fills.
                var bidDepth = buyOrders.Skip(buyIndex).Sum(o => o.Quantity - o.FilledQuantity);
                var askDepth = sellOrders.Skip(sellIndex).Sum(o => o.Quantity - o.FilledQuantity);

                var trade = TradeExecution.Create(
                    bestBuy.Id,
                    request.MarketId,
                    bestBuy.OutcomeId,
                    bestBuy.Id,
                    bestSell.Id,
                    bestBuy.UserId,
                    tradeShares,
                    executedPrice);

                bestBuy.TryFill(tradeShares, executedPrice);
                bestSell.TryFill(tradeShares, executedPrice);

                _dbContext.TradeExecutions.Add(trade);
                _dbContext.TradeFlowSnapshots.Add(TradeFlowSnapshot.Create(
                    request.MarketId, bestBuy.OutcomeId, trade.CreatedAt, executedPrice, tradeShares,
                    bestBuy.Id, bestSell.Id, bidDepth, askDepth));

                await ProcessTradeExecution(trade, bestBuy, bestSell, cancellationToken);

                executedTrades.Add(trade);
                fills.Add(new Fill(
                    request.MarketId, bestBuy.OutcomeId, bestBuy.Price, bestSell.Price,
                    executedPrice, tradeShares, trade.CreatedAt));
                totalVolume += trade.TotalValue;

                if (bestBuy.Status == OrderStatus.Filled)
                    buyIndex++;
                
                if (bestSell.Status == OrderStatus.Filled)
                    sellIndex++;
            }
            else
            {
                break;
            }
        }

        if (executedTrades.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await PublishFills(fills, cancellationToken);
        }

        await ExpireExpiredOrders(request.MarketId, cancellationToken);

        return new MatchingResult(
            executedTrades.Count,
            totalVolume,
            executedTrades);
    }

    /// <summary>Reflects committed fills in the live order book cache and broadcasts them to clients.</summary>
    private async Task PublishFills(List<Fill> fills, CancellationToken cancellationToken)
    {
        foreach (var fill in fills)
        {
            // Each matched share leaves the book on both sides at the resting price levels.
            await _orderBookCache.ReduceRestingQuantityAsync(
                fill.OutcomeId, OrderSide.Buy, fill.BuyPrice, fill.Shares, cancellationToken);
            await _orderBookCache.ReduceRestingQuantityAsync(
                fill.OutcomeId, OrderSide.Sell, fill.SellPrice, fill.Shares, cancellationToken);

            await _orderBookCache.SetLatestPriceAsync(
                fill.OutcomeId, fill.ExecutedPrice, fill.Shares, fill.Timestamp, cancellationToken);

            await _realtimePublisher.PublishTradeAsync(
                new TradeExecutedEvent(fill.MarketId, fill.OutcomeId, fill.ExecutedPrice, fill.Shares, fill.Timestamp),
                cancellationToken);
            await _realtimePublisher.PublishPriceUpdateAsync(
                new PriceUpdateEvent(fill.OutcomeId, fill.ExecutedPrice, fill.Shares, fill.Timestamp),
                cancellationToken);
        }
    }

    /// <summary>Updates buyer's position (create/increase) and seller's position (reduce), then handles payments.</summary>
    private async Task ProcessTradeExecution(
        TradeExecution trade, 
        LimitOrder buyerOrder, 
        LimitOrder sellerOrder,
        CancellationToken cancellationToken)
    {
        var existingPosition = await _dbContext.Positions
            .FirstOrDefaultAsync(p => 
                p.UserId == buyerOrder.UserId && 
                p.OutcomeId == trade.OutcomeId, cancellationToken);

        if (existingPosition is null)
        {
            var newPosition = Position.Create(
                buyerOrder.UserId,
                trade.OutcomeId,
                trade.TotalValue,
                trade.Shares,
                trade.ExecutedPrice,
                trade.ExecutedPrice);

            _dbContext.Positions.Add(newPosition);
        }
        else
        {
            existingPosition.UpdatePosition(trade.Shares, trade.TotalValue, trade.ExecutedPrice);
        }

        var sellerPosition = await _dbContext.Positions
            .FirstOrDefaultAsync(p =>
                p.UserId == sellerOrder.UserId &&
                p.OutcomeId == trade.OutcomeId, cancellationToken);

        // A sell order must always have a backing position. If it is missing the sell
        // order was placed against shares that no longer exist, crediting the seller
        // here would mint money from nothing.
        if (sellerPosition is null)
            throw new InvalidOperationException(
                $"Seller position not found for user {sellerOrder.UserId} on outcome {trade.OutcomeId}. Trade aborted.");

        sellerPosition.ReducePosition(trade.Shares);

        var overpayment = (buyerOrder.Price - trade.ExecutedPrice) * trade.Shares;
        if (overpayment > 0)
            await _walletService.CreditBalance(buyerOrder.UserId, overpayment);
        await _walletService.CreditBalance(sellerOrder.UserId, trade.TotalValue);
    }

    /// <summary>Marks expired orders and refunds unfilled buy portions.</summary>
    private async Task ExpireExpiredOrders(Guid marketId, CancellationToken cancellationToken)
    {
        var allOrders = await _dbContext.LimitOrders
            .Where(o => o.MarketId == marketId)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var expiredOrders = allOrders
            .Where(o => o.ExpiresAt.HasValue
                        && o.ExpiresAt.Value < now
                        && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.PartiallyFilled))
            .ToList();

        foreach (var order in expiredOrders)
        {
            order.MarkExpired();

            if (order.Side == OrderSide.Buy)
            {
                var unfilledQuantity = order.Quantity - order.FilledQuantity;
                var refundAmount = unfilledQuantity * order.Price;

                if (refundAmount > 0)
                    await _walletService.CreditBalance(order.UserId, refundAmount);
            }
        }

        if (expiredOrders.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Remove expired remainders from the live order book.
            foreach (var order in expiredOrders)
            {
                await _orderBookCache.ReduceRestingQuantityAsync(
                    order.OutcomeId, order.Side, order.Price,
                    order.Quantity - order.FilledQuantity, cancellationToken);
            }
        }
    }
}
