using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Favorites.Commands;

/// <summary>Command to toggle a team as a favorite for a user.</summary>
public record ToggleFavoriteTeamCommand(Guid UserId, Guid TeamId) : IRequest<bool>;

/// <summary>Handles toggling a favorite team.</summary>
public class ToggleFavoriteTeamCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleFavoriteTeamCommand, bool>
{
    /// <summary>Toggles the favorite status and returns the new state.</summary>
    public async Task<bool> Handle(ToggleFavoriteTeamCommand request, CancellationToken cancellationToken)
    {
        var existing = await context.FavoriteTeams
            .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.TeamId == request.TeamId,
                cancellationToken);

        if (existing is not null)
        {
            context.FavoriteTeams.Remove(existing);
            await context.SaveChangesAsync(cancellationToken);
            return false;
        }

        context.FavoriteTeams.Add(FavoriteTeam.Create(request.UserId, request.TeamId));
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
