using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Leagues.Commands;

/// <summary>Command to create a new league.</summary>
public record CreateLeagueCommand(
    string Name,
    string Slug,
    Guid SportId,
    string? LogoUrl = null,
    string? Country = null) : IRequest<Guid>;

/// <summary>Handles the creation of a league.</summary>
public class CreateLeagueCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateLeagueCommand, Guid>
{
    /// <summary>Creates the league and returns its ID.</summary>
    public async Task<Guid> Handle(CreateLeagueCommand request, CancellationToken cancellationToken)
    {
        var league = League.Create(request.Name, request.Slug, request.SportId, request.LogoUrl, request.Country);
        context.Leagues.Add(league);
        await context.SaveChangesAsync(cancellationToken);
        return league.Id;
    }
}
