using AnchorMarket.Application.Common.Interfaces;
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

    public MatchOrdersCommandHandler(
        IApplicationDbContext dbContext,
        IWalletService walletService)
    {
        _dbContext = dbContext;
        _walletService = walletService;
    }

    /// <summary>Matches buy and sell orders and returns the execution result.</summary>
    public async Task<MatchingResult> Handle(MatchOrdersCommand request, CancellationToken cancellationToken)
    {
        var executedTrades = new List<TradeExecution>();
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

                await ProcessTradeExecution(trade, bestBuy, bestSell, cancellationToken);

                executedTrades.Add(trade);
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
            await _dbContext.SaveChangesAsync(cancellationToken);

        await ExpireExpiredOrders(request.MarketId, cancellationToken);

        return new MatchingResult(
            executedTrades.Count,
            totalVolume,
            executedTrades);
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

        if (sellerPosition is not null)
        {
            sellerPosition.ReducePosition(trade.Shares);
        }

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

        if (expiredOrders.Any())
            await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
