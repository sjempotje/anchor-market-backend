using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Orders.Commands;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AnchorMarket.Application.Features.Positions.Commands;

/// <summary>Command to place a market-order position on a specific outcome.</summary>
public record PlacePositionCommand(
    Guid UserId,
    Guid MarketId,
    Guid OutcomeId,
    decimal Amount) : IRequest<Guid>;

/// <summary>Handles placing a position.</summary>
public class PlacePositionCommandHandler : IRequestHandler<PlacePositionCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IWalletService _walletService;

    public PlacePositionCommandHandler(IApplicationDbContext dbContext, IWalletService walletService)
    {
        _dbContext = dbContext;
        _walletService = walletService;
    }

    /// <summary>Calculates the current price, debits the user, and creates the position.</summary>
    public async Task<Guid> Handle(PlacePositionCommand request, CancellationToken cancellationToken)
    {
        var market = await _dbContext.Markets
            .Include(m => m.Outcomes)
            .FirstOrDefaultAsync(m => m.Id == request.MarketId, cancellationToken);

        if (market is null)
            throw new NotFoundException("Market not found.");

        if (market.Status != MarketStatus.Open)
            throw new InvalidOperationException($"Market is not open. Status: {market.Status}");

        var outcome = market.Outcomes.FirstOrDefault(o => o.Id == request.OutcomeId);
        if (outcome is null)
            throw new NotFoundException("Outcome not found for this market.");

        var existingPositions = await _dbContext.Positions
            .Where(p => p.OutcomeId == request.OutcomeId)
            .ToListAsync(cancellationToken);

        var currentPrice = existingPositions.Any()
            ? existingPositions.Sum(p => p.EntryPrice * p.Shares) / existingPositions.Sum(p => p.Shares)
            : 0.5m;

        var shares = request.Amount / currentPrice;

        await _walletService.DebitBalance(request.UserId, request.Amount);

        var position = Position.Create(
            request.UserId,
            request.OutcomeId,
            request.Amount,
            shares,
            currentPrice,
            currentPrice);

        _dbContext.Positions.Add(position);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return position.Id;
    }
}
