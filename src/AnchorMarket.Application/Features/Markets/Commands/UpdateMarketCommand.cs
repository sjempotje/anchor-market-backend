using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Markets.Commands;

public record UpdateMarketCommand(
    Guid MarketId,
    string Title,
    string Description,
    DateTimeOffset ResolutionDeadline) : IRequest;

public class UpdateMarketCommandHandler : IRequestHandler<UpdateMarketCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateMarketCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateMarketCommand request, CancellationToken cancellationToken)
    {
        var market = await _context.Markets.FindAsync([request.MarketId], cancellationToken);

        if (market is null)
            throw new NotFoundException($"Market with ID {request.MarketId} not found.");

        market.Update(request.Title, request.Description, request.ResolutionDeadline);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
