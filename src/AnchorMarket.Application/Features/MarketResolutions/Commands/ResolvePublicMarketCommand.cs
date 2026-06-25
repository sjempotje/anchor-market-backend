using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Realtime;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.MarketResolutions.Commands;

/// <summary>Command to resolve a public market with a winning outcome (admin action).</summary>
public record ResolvePublicMarketCommand(
    Guid MarketId,
    Guid WinningOutcomeId,
    Guid ResolvedById,
    string? ResolutionSource = null,
    string? ResolutionNotes = null) : IRequest;

/// <summary>Handles resolving a public market.</summary>
public class ResolvePublicMarketCommandHandler(
    IApplicationDbContext context,
    IRealtimePublisher realtimePublisher) : IRequestHandler<ResolvePublicMarketCommand>
{
    /// <summary>Records the resolution, settles position fair values, and broadcasts the result.</summary>
    public async Task Handle(ResolvePublicMarketCommand request, CancellationToken cancellationToken)
    {
        var market = await context.Markets
            .Include(m => m.Outcomes)
            .FirstOrDefaultAsync(m => m.Id == request.MarketId, cancellationToken)
            ?? throw new NotFoundException($"Market with ID {request.MarketId} not found.");

        if (market.Scope == MarketScope.Group)
            throw new InvalidOperationException("Group markets must be resolved by a group member via the group market endpoint.");

        if (market.Status != MarketStatus.Open)
            throw new InvalidOperationException($"Market is not open. Status: {market.Status}");

        if (market.Outcomes.All(o => o.Id != request.WinningOutcomeId))
            throw new NotFoundException("Winning outcome not found for this market.");

        context.MarketResolutions.Add(
            MarketResolution.Create(market.Id, request.WinningOutcomeId, request.ResolvedById));

        var outcomeIds = market.Outcomes.Select(o => o.Id).ToList();
        var positions = await context.Positions
            .Where(p => outcomeIds.Contains(p.OutcomeId))
            .ToListAsync(cancellationToken);

        foreach (var position in positions)
            position.UpdateFairValue(position.OutcomeId == request.WinningOutcomeId ? 1.0m : 0.0m);

        if (request.ResolutionSource is not null || request.ResolutionNotes is not null)
            market.SetResolutionSource(request.ResolutionSource, request.ResolutionNotes);

        market.MarkAsResolved();
        await context.SaveChangesAsync(cancellationToken);

        await realtimePublisher.PublishMarketResolvedAsync(
            new MarketResolvedEvent(market.Id, market.GroupId, request.WinningOutcomeId, DateTimeOffset.UtcNow),
            cancellationToken);
    }
}
