using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Orders.Commands;

public record PlaceLimitOrderCommand(
    Guid UserId,
    Guid MarketId,
    Guid OutcomeId,
    OrderSide Side,
    decimal Price,
    decimal Quantity,
    DateTimeOffset? ExpiresAt) : IRequest<Guid>;

public class PlaceLimitOrderCommandHandler(
    IApplicationDbContext dbContext,
    IWalletService walletService) : IRequestHandler<PlaceLimitOrderCommand, Guid>
{
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
            var existingPosition = await dbContext.Positions.FirstOrDefaultAsync(
                p => p.UserId == request.UserId && p.OutcomeId == request.OutcomeId, cancellationToken);

            if (existingPosition is null || existingPosition.Shares < request.Quantity)
                throw new InvalidOperationException("Insufficient shares to sell.");

            order = LimitOrder.CreateSell(
                request.MarketId, request.OutcomeId, request.UserId,
                request.Price, request.Quantity, request.ExpiresAt);
        }

        dbContext.LimitOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}

public interface IWalletService
{
    Task DebitBalance(Guid userId, decimal amount);
    Task CreditBalance(Guid userId, decimal amount);
}
