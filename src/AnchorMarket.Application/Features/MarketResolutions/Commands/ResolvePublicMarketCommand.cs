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
    IRealtimePublisher realtimePublisher,
    IWalletService walletService) : IRequestHandler<ResolvePublicMarketCommand>
{
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

        var losingPositions = positions.Where(p => p.OutcomeId != request.WinningOutcomeId).ToList();
        var winningPositions = positions.Where(p => p.OutcomeId == request.WinningOutcomeId).ToList();

        var loserPool = losingPositions.Sum(p => p.Amount);

        if (winningPositions.Count > 0 && loserPool > 0)
        {
            var payoutPerWinner = loserPool / winningPositions.Count;
            foreach (var position in winningPositions)
            {
                position.Resolve(position.Amount + payoutPerWinner);
                await walletService.CreditBalance(position.UserId, position.Amount + payoutPerWinner);
            }
        }

        foreach (var position in losingPositions)
            position.Resolve(0);

        if (request.ResolutionSource is not null || request.ResolutionNotes is not null)
            market.SetResolutionSource(request.ResolutionSource, request.ResolutionNotes);

        market.MarkAsResolved();
        await context.SaveChangesAsync(cancellationToken);

        await realtimePublisher.PublishMarketResolvedAsync(
            new MarketResolvedEvent(market.Id, market.GroupId, request.WinningOutcomeId, DateTimeOffset.UtcNow),
            cancellationToken);
    }
}
