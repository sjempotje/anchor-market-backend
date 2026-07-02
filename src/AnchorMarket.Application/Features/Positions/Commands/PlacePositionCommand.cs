using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Realtime;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Positions.Commands;

/// <summary>Command to place a bet on a market outcome.</summary>
public record PlacePositionCommand(
    Guid UserId,
    Guid MarketId,
    Guid OutcomeId,
    decimal Amount) : IRequest<Guid>;

/// <summary>Handles placing a bet.</summary>
public class PlacePositionCommandHandler : IRequestHandler<PlacePositionCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IWalletService _walletService;
    private readonly IRealtimePublisher _realtimePublisher;

    public PlacePositionCommandHandler(
        IApplicationDbContext dbContext,
        IWalletService walletService,
        IRealtimePublisher realtimePublisher)
    {
        _dbContext = dbContext;
        _walletService = walletService;
        _realtimePublisher = realtimePublisher;
    }

    public async Task<Guid> Handle(PlacePositionCommand request, CancellationToken cancellationToken)
    {
        var market = await _dbContext.Markets
            .Include(m => m.Outcomes)
            .FirstOrDefaultAsync(m => m.Id == request.MarketId, cancellationToken);

        if (market is null)
            throw new NotFoundException("Market not found.");

        if (market.Status != MarketStatus.Open)
            throw new InvalidOperationException($"Market is not open. Status: {market.Status}");

        if (request.Amount < 0.01m)
            throw new InvalidOperationException("Amount must be at least 0.01.");

        var outcome = market.Outcomes.FirstOrDefault(o => o.Id == request.OutcomeId);
        if (outcome is null)
            throw new NotFoundException("Outcome not found for this market.");

        await _walletService.DebitBalance(request.UserId, request.Amount);

        var position = Position.Create(request.UserId, request.OutcomeId, market.Id, request.Amount);

        _dbContext.Positions.Add(position);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Calculate and publish price updates for all outcomes (prices are interdependent)
        var marketTotalAfter = market.TotalBetAmount + request.Amount;
        var now = DateTimeOffset.UtcNow;

        var priceUpdateTasks = market.Outcomes.Select(async o =>
        {
            var outcomeTotalAfter = o.Id == request.OutcomeId
                ? o.TotalBetAmount + request.Amount
                : o.TotalBetAmount;

            var newPrice = marketTotalAfter > 0
                ? outcomeTotalAfter / marketTotalAfter
                : 1m / market.Outcomes.Count;

            var priceUpdate = new PriceUpdateEvent(o.Id, newPrice, request.Amount, now);
            await _realtimePublisher.PublishPriceUpdateAsync(priceUpdate, cancellationToken);
        });

        await Task.WhenAll(priceUpdateTasks);

        return position.Id;
    }
}
