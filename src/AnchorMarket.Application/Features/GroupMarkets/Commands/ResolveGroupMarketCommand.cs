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

    public ResolveGroupMarketCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Resolves the market, records the resolution, and updates position fair values.</summary>
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

        var resolution = MarketResolution.Create(market.Id, request.WinningOutcomeId, request.ResolverId);
        _context.MarketResolutions.Add(resolution);

        var outcomeIds = market.Outcomes.Select(o => o.Id).ToList();
        var positions = await _context.Positions
            .Where(p => outcomeIds.Contains(p.OutcomeId))
            .ToListAsync(cancellationToken);

        foreach (var position in positions)
        {
            var isWinning = position.OutcomeId == request.WinningOutcomeId;
            position.UpdateFairValue(isWinning ? 1.0m : 0.0m);
        }

        market.MarkAsResolved();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
