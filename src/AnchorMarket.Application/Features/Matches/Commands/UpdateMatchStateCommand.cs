using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Matches.Commands;

/// <summary>Command to update the live state of a match.</summary>
public record UpdateMatchStateCommand(
    Guid MatchId,
    int ScoreHome,
    int ScoreAway,
    string? CurrentPeriod = null,
    string? Clock = null,
    string? ExtraInfo = null) : IRequest;

/// <summary>Handles updating the match state.</summary>
public class UpdateMatchStateCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateMatchStateCommand>
{
    /// <summary>Updates or creates the match state record.</summary>
    public async Task Handle(UpdateMatchStateCommand request, CancellationToken cancellationToken)
    {
        var state = await context.MatchStates
            .FirstOrDefaultAsync(s => s.MatchId == request.MatchId, cancellationToken);

        if (state is null)
        {
            var matchExists = await context.Matches
                .AnyAsync(m => m.Id == request.MatchId, cancellationToken);
            if (!matchExists)
                throw new NotFoundException($"Match {request.MatchId} not found.");

            state = MatchState.Create(request.MatchId);
            context.MatchStates.Add(state);
        }

        state.Update(request.ScoreHome, request.ScoreAway, request.CurrentPeriod, request.Clock, request.ExtraInfo);
        await context.SaveChangesAsync(cancellationToken);
    }
}
