using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.GroupMarkets.Commands;

/// <summary>Command to resolve a group market with a winning outcome.</summary>
public record ResolveGroupMarketCommand(
    Guid MarketId,
    Guid WinningOutcomeId,
    Guid ResolverId) : IRequest;

/// <summary>Handles resolving a group market.</summary>
public class ResolveGroupMarketCommandHandler : IRequestHandler<ResolveGroupMarketCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IWalletService _walletService;

    public ResolveGroupMarketCommandHandler(IApplicationDbContext context, IWalletService walletService)
    {
        _context = context;
        _walletService = walletService;
    }

    public async Task Handle(ResolveGroupMarketCommand request, CancellationToken cancellationToken)
    {
        var market = await _context.Markets
            .Include(m => m.Outcomes)
            .Include(m => m.Group)
            .FirstOrDefaultAsync(m => m.Id == request.MarketId, cancellationToken);

        if (market is null)
            throw new NotFoundException($"Market with ID {request.MarketId} not found.");

        if (market.Status != MarketStatus.Open)
            throw new InvalidOperationException($"Market is not open. Status: {market.Status}");

        var outcome = market.Outcomes.FirstOrDefault(o => o.Id == request.WinningOutcomeId);
        if (outcome is null)
            throw new NotFoundException("Winning outcome not found for this market.");

        var isMember = await _context.GroupMemberships
            .AnyAsync(m => m.GroupId == market.GroupId && m.UserId == request.ResolverId, cancellationToken);
        if (!isMember)
            throw new InvalidOperationException("Only group members can resolve group markets.");

        if (market.CreatorId == request.ResolverId)
            throw new InvalidOperationException("Resolver cannot be the market creator.");

        _context.MarketResolutions.Add(MarketResolution.Create(market.Id, request.WinningOutcomeId, request.ResolverId));

        var outcomeIds = market.Outcomes.Select(o => o.Id).ToList();
        var positions = await _context.Positions
            .Where(p => outcomeIds.Contains(p.OutcomeId))
            .ToListAsync(cancellationToken);

        var losingPositions = positions.Where(p => p.OutcomeId != request.WinningOutcomeId).ToList();
        var winningPositions = positions.Where(p => p.OutcomeId == request.WinningOutcomeId).ToList();

        var loserPool = losingPositions.Sum(p => p.Amount);

        if (winningPositions.Count > 0 && loserPool > 0)
        {
            var payoutPerWinner = loserPool / winningPositions.Count;
            foreach (var position in winningPositions)
            {
                position.Resolve(position.Amount + payoutPerWinner);
                await _walletService.CreditBalance(position.UserId, position.Amount + payoutPerWinner);
            }
        }

        foreach (var position in losingPositions)
            position.Resolve(0);

        market.MarkAsResolved();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
