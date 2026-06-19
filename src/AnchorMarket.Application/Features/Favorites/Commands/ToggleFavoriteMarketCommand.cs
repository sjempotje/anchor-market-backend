using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Favorites.Commands;

/// <summary>Command to toggle a market as a favorite for a user.</summary>
public record ToggleFavoriteMarketCommand(Guid UserId, Guid MarketId) : IRequest<bool>;

/// <summary>Handles toggling a favorite market.</summary>
public class ToggleFavoriteMarketCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleFavoriteMarketCommand, bool>
{
    /// <summary>Toggles the favorite status and returns the new state.</summary>
    public async Task<bool> Handle(ToggleFavoriteMarketCommand request, CancellationToken cancellationToken)
    {
        var existing = await context.FavoriteMarkets
            .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.MarketId == request.MarketId,
                cancellationToken);

        if (existing is not null)
        {
            context.FavoriteMarkets.Remove(existing);
            await context.SaveChangesAsync(cancellationToken);
            return false;
        }

        context.FavoriteMarkets.Add(FavoriteMarket.Create(request.UserId, request.MarketId));
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
