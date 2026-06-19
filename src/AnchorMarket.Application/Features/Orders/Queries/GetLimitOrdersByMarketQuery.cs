using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Orders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Orders.Queries;

/// <summary>Query to retrieve limit orders for a market, optionally filtered by outcome and user.</summary>
public record GetLimitOrdersByMarketQuery(
    Guid MarketId,
    Guid? OutcomeId = null,
    Guid? UserId = null) : IRequest<IReadOnlyList<LimitOrderDto>>;

/// <summary>Handles retrieving limit orders for a market.</summary>
public class GetLimitOrdersByMarketQueryHandler : IRequestHandler<GetLimitOrdersByMarketQuery, IReadOnlyList<LimitOrderDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetLimitOrdersByMarketQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Returns the matching limit orders, ordered by newest first.</summary>
    public async Task<IReadOnlyList<LimitOrderDto>> Handle(
        GetLimitOrdersByMarketQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.LimitOrders
            .Where(o => o.MarketId == request.MarketId);

        if (request.OutcomeId.HasValue)
            query = query.Where(o => o.OutcomeId == request.OutcomeId.Value);

        if (request.UserId.HasValue)
            query = query.Where(o => o.UserId == request.UserId.Value);

        var orders = await query
            .ToListAsync(cancellationToken);

        var sortedOrders = orders.OrderByDescending(o => o.CreatedAt).ToList();

        return sortedOrders.Select(o => new LimitOrderDto(
            o.Id, o.MarketId, o.OutcomeId, o.UserId, o.Side,
            o.Price, o.Quantity, o.FilledQuantity, o.TotalCost,
            o.Type, o.Status, o.ExpiresAt, o.CreatedAt, o.UpdatedAt))
            .ToList();
    }
}

/// <summary>Query to retrieve detailed limit order information including trade executions.</summary>
public record GetLimitOrderDetailQuery(Guid OrderId, Guid UserId) : IRequest<LimitOrderDetailDto>;

/// <summary>Handles retrieving detailed limit order information.</summary>
public class GetLimitOrderDetailQueryHandler : IRequestHandler<GetLimitOrderDetailQuery, LimitOrderDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetLimitOrderDetailQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Returns the detailed order if owned by the caller.</summary>
    public async Task<LimitOrderDetailDto> Handle(
        GetLimitOrderDetailQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.LimitOrders
            .Include(o => o.Market)
            .Include(o => o.Outcome)
            .Include(o => o.TradeExecutions)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            throw new NotFoundException("Order not found.");

        if (order.UserId != request.UserId)
            throw new ForbiddenException("You do not own this order.");

        return new LimitOrderDetailDto(
            order.Id,
            order.MarketId,
            order.Market.Title,
            order.OutcomeId,
            order.Outcome.Title,
            order.UserId,
            order.Side,
            order.Price,
            order.Quantity,
            order.FilledQuantity,
            order.Quantity - order.FilledQuantity,
            order.FilledQuantity > 0 ? order.TotalCost / order.FilledQuantity : 0m,
            order.TotalCost,
            order.Type,
            order.Status,
            order.ExpiresAt,
            order.CreatedAt,
            order.UpdatedAt,
            order.TradeExecutions.Select(te => new TradeExecutionDto(
                te.Id, te.LimitOrderId, te.BuyerOrderId, te.SellerOrderId,
                te.InitiatorUserId, te.Shares, te.ExecutedPrice,
                te.TotalValue, te.CreatedAt)).ToList());
    }
}
