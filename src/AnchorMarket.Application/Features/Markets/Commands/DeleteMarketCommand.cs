using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Markets.Commands;

/// <summary>Command to delete a market.</summary>
public record DeleteMarketCommand(Guid MarketId) : IRequest;

/// <summary>Handles the deletion of a market.</summary>
public class DeleteMarketCommandHandler : IRequestHandler<DeleteMarketCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteMarketCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Deletes the market if it exists.</summary>
    public async Task Handle(DeleteMarketCommand request, CancellationToken cancellationToken)
    {
        var market = await _context.Markets.FindAsync([request.MarketId], cancellationToken);

        if (market is null)
            throw new NotFoundException($"Market with ID {request.MarketId} not found.");

        _context.Markets.Remove(market);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
