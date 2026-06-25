using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Orders.Commands;

/// <summary>Command to cancel a limit order.</summary>
public record CancelLimitOrderCommand(Guid OrderId, Guid UserId) : IRequest;

/// <summary>Handles cancelling a limit order.</summary>
public class CancelLimitOrderCommandHandler : IRequestHandler<CancelLimitOrderCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IWalletService _walletService;
    private readonly IOrderBookCache _orderBookCache;

    public CancelLimitOrderCommandHandler(
        IApplicationDbContext dbContext,
        IWalletService walletService,
        IOrderBookCache orderBookCache)
    {
        _dbContext = dbContext;
        _walletService = walletService;
        _orderBookCache = orderBookCache;
    }

    /// <summary>Cancels the order and refunds unfilled buy portion.</summary>
    public async Task Handle(CancelLimitOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.LimitOrders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            throw new NotFoundException("Order not found.");

        if (order.UserId != request.UserId)
            throw new ForbiddenException("You do not own this order.");

        if (order.Status == Domain.Enums.OrderStatus.Filled)
            throw new InvalidOperationException("Cannot cancel a fully filled order.");

        var unfilledQuantity = order.Quantity - order.FilledQuantity;

        if (order.Side == Domain.Enums.OrderSide.Buy)
        {
            var refundAmount = unfilledQuantity * order.Price;

            if (refundAmount > 0)
                await _walletService.CreditBalance(request.UserId, refundAmount);
        }

        order.Cancel();
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Remove the now-cancelled remainder from the live order book.
        await _orderBookCache.ReduceRestingQuantityAsync(
            order.OutcomeId, order.Side, order.Price, unfilledQuantity, cancellationToken);
    }
}
