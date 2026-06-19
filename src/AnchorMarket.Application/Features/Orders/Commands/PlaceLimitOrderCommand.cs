using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AnchorMarket.Application.Features.Orders.Commands;

/// <summary>Command to place a limit order on a market outcome.</summary>
public record PlaceLimitOrderCommand(
    Guid UserId,
    Guid MarketId,
    Guid OutcomeId,
    OrderSide Side,
    decimal Price,
    decimal Quantity,
    DateTimeOffset? ExpiresAt) : IRequest<Guid>;

/// <summary>Handles placing a limit order.</summary>
public class PlaceLimitOrderCommandHandler(
    IApplicationDbContext dbContext,
    IWalletService walletService) : IRequestHandler<PlaceLimitOrderCommand, Guid>
{
    /// <summary>Validates the order, debits balance, and returns the order ID.</summary>
    public async Task<Guid> Handle(PlaceLimitOrderCommand request, CancellationToken cancellationToken)
    {
        var market = await dbContext.Markets
            .Include(m => m.Outcomes)
            .FirstOrDefaultAsync(m => m.Id == request.MarketId, cancellationToken);

        if (market is null)
            throw new NotFoundException("Market not found.");

        if (market.Status != MarketStatus.Open)
            throw new InvalidOperationException($"Market is not open. Status: {market.Status}");

        var outcome = market.Outcomes.FirstOrDefault(o => o.Id == request.OutcomeId);
        if (outcome is null)
            throw new NotFoundException("Outcome not found for this market.");

        if (request.Price < 0.01m || request.Price > 0.99m)
            throw new InvalidOperationException("Price must be between 0.01 and 0.99.");

        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        var orderCost = request.Quantity * request.Price;
        LimitOrder order;
        
        if (request.Side == OrderSide.Buy)
        {
            await walletService.DebitBalance(request.UserId, orderCost);
            order = LimitOrder.CreateBuy(
                request.MarketId, request.OutcomeId, request.UserId,
                request.Price, request.Quantity, request.ExpiresAt);
        }
        else
        {
            // Explicit transaction ensures the availability read and order insert are
            // atomic. On PostgreSQL this runs at Serializable isolation to prevent
            // concurrent sell requests from double-booking the same shares.
            await using var tx = await dbContext.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            var existingPosition = await dbContext.Positions.FirstOrDefaultAsync(
                p => p.UserId == request.UserId && p.OutcomeId == request.OutcomeId, cancellationToken);

            if (existingPosition is null)
                throw new InvalidOperationException("Insufficient shares to sell.");

            var pendingLockedShares = await dbContext.LimitOrders
                .Where(o => o.UserId == request.UserId
                         && o.OutcomeId == request.OutcomeId
                         && o.Side == OrderSide.Sell
                         && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.PartiallyFilled))
                .SumAsync(o => o.Quantity - o.FilledQuantity, cancellationToken);

            var availableShares = existingPosition.Shares - pendingLockedShares;
            if (availableShares < request.Quantity)
                throw new InvalidOperationException("Insufficient shares to sell.");

            order = LimitOrder.CreateSell(
                request.MarketId, request.OutcomeId, request.UserId,
                request.Price, request.Quantity, request.ExpiresAt);

            dbContext.LimitOrders.Add(order);
            await dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return order.Id;
        }

        dbContext.LimitOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}

/// <summary>Service for managing wallet debits and credits.</summary>
public interface IWalletService
{
    /// <summary>Debits (locks) funds from a user's wallet.</summary>
    Task DebitBalance(Guid userId, decimal amount);
    /// <summary>Credits (returns) funds to a user's wallet.</summary>
    Task CreditBalance(Guid userId, decimal amount);
}
