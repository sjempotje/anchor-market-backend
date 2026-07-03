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

        var liveTotalsByOutcome = await _dbContext.Positions
            .Where(p => p.MarketId == market.Id)
            .GroupBy(p => p.OutcomeId)
            .Select(g => new { OutcomeId = g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.OutcomeId, x => x.Total, cancellationToken);

        var marketTotalAfter = liveTotalsByOutcome.Values.Sum();
        var now = DateTimeOffset.UtcNow;

        var newPrices = market.Outcomes.ToDictionary(
            o => o.Id,
            o =>
            {
                var outcomeTotalAfter = liveTotalsByOutcome.GetValueOrDefault(o.Id, 0m);

                return marketTotalAfter > 0
                    ? outcomeTotalAfter / marketTotalAfter
                    : 1m / market.Outcomes.Count;
            });

        foreach (var o in market.Outcomes)
        {
            _dbContext.OutcomePricePoints.Add(
                OutcomePricePoint.Create(o.Id, newPrices[o.Id], request.Amount, isTrade: o.Id == request.OutcomeId));
        }
        await _dbContext.SaveChangesAsync(cancellationToken);

        var priceUpdateTasks = market.Outcomes.Select(o =>
            _realtimePublisher.PublishPriceUpdateAsync(
                new PriceUpdateEvent(o.Id, newPrices[o.Id], request.Amount, now), cancellationToken));

        await Task.WhenAll(priceUpdateTasks);

        await _realtimePublisher.PublishTradeAsync(
            new TradeExecutedEvent(market.Id, request.OutcomeId, newPrices[request.OutcomeId], request.Amount, now),
            cancellationToken);

        return position.Id;
    }
}
