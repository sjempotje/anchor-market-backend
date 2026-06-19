using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Positions.Commands;

/// <summary>Command to close (sell) a user's open position.</summary>
public record ClosePositionCommand(Guid PositionId, Guid UserId) : IRequest;

/// <summary>Handles closing a position.</summary>
public class ClosePositionCommandHandler : IRequestHandler<ClosePositionCommand>
{
    private readonly IApplicationDbContext _context;

    public ClosePositionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Closes the position and credits the return amount to the wallet.</summary>
    public async Task Handle(ClosePositionCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.Positions
            .Include(p => p.Outcome.Market)
            .FirstOrDefaultAsync(p => p.Id == request.PositionId && p.UserId == request.UserId, cancellationToken);

        if (position is null)
            throw new NotFoundException($"Position with ID {request.PositionId} not found.");

        var marketStatus = position.Outcome.Market.Status;
        if (marketStatus != MarketStatus.Open && marketStatus != MarketStatus.Resolved)
            throw new InvalidOperationException($"Cannot close a position on a {marketStatus} market.");

        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);
        if (wallet is not null)
        {
            // Resolved markets: pay out at settlement fair value (1.0 for winners, 0.0 for losers).
            // Open markets: return the original cost basis so early exits can't exploit a
            // manipulated CurrentFairValue that hasn't been through the resolution process.
            var returnAmount = marketStatus == MarketStatus.Resolved
                ? position.Shares * position.CurrentFairValue
                : position.Shares * position.EntryPrice;

            wallet.Credit(returnAmount);
        }

        _context.Positions.Remove(position);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
