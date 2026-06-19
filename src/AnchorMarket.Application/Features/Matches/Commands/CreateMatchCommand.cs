using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Matches.Commands;

/// <summary>Command to create a new match.</summary>
public record CreateMatchCommand(
    Guid HomeTeamId,
    Guid AwayTeamId,
    Guid LeagueId,
    DateTimeOffset StartTime,
    Guid? EventId = null) : IRequest<Guid>;

/// <summary>Handles the creation of a match.</summary>
public class CreateMatchCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateMatchCommand, Guid>
{
    /// <summary>Creates the match and returns its ID.</summary>
    public async Task<Guid> Handle(CreateMatchCommand request, CancellationToken cancellationToken)
    {
        var match = Match.Create(request.HomeTeamId, request.AwayTeamId, request.LeagueId,
            request.StartTime, request.EventId);
        context.Matches.Add(match);
        await context.SaveChangesAsync(cancellationToken);
        return match.Id;
    }
}
