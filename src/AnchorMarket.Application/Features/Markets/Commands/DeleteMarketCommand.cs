using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Markets.Commands;

/// <summary>Command to delete a market. Only the market's creator may delete it.</summary>
public record DeleteMarketCommand(Guid MarketId, Guid CallerId) : IRequest;

/// <summary>Handles the deletion of a market.</summary>
public class DeleteMarketCommandHandler : IRequestHandler<DeleteMarketCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteMarketCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Deletes the market if it exists and the caller is the creator.</summary>
    public async Task Handle(DeleteMarketCommand request, CancellationToken cancellationToken)
    {
        var market = await _context.Markets.FindAsync([request.MarketId], cancellationToken);

        if (market is null)
            throw new NotFoundException($"Market with ID {request.MarketId} not found.");

        if (market.CreatorId != request.CallerId)
            throw new ForbiddenException("Only the market creator can delete this market.");

        _context.Markets.Remove(market);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
