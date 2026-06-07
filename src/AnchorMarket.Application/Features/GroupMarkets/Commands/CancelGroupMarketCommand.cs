using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.GroupMarkets.Commands;

/// <summary>
/// Cancels an open group market. Only the creator or a group admin may cancel.
/// Business rule enforced in the handler.
/// </summary>
public record CancelGroupMarketCommand(Guid MarketId, Guid RequestingUserId) : IRequest;

public class CancelGroupMarketCommandHandler : IRequestHandler<CancelGroupMarketCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelGroupMarketCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CancelGroupMarketCommand request, CancellationToken cancellationToken)
    {
        var market = await _context.Markets.FindAsync([request.MarketId], cancellationToken);

        if (market is null)
            throw new NotFoundException($"Market with ID {request.MarketId} not found.");

        if (market.CreatorId != request.RequestingUserId)
            throw new InvalidOperationException("Only the market creator can cancel this market.");

        if (market.Status != Domain.Enums.MarketStatus.Open)
            throw new InvalidOperationException($"Market is not open. Status: {market.Status}");

        market.MarkAsCancelled();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
