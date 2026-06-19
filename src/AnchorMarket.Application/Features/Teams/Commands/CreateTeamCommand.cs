using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Teams.Commands;

/// <summary>Command to create a new team.</summary>
public record CreateTeamCommand(
    string Name,
    string ShortName,
    string Slug,
    Guid SportId,
    string? LogoUrl = null,
    string? Country = null,
    string? CountryCode = null) : IRequest<Guid>;

/// <summary>Handles the creation of a team.</summary>
public class CreateTeamCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateTeamCommand, Guid>
{
    /// <summary>Creates the team and returns its ID.</summary>
    public async Task<Guid> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = Team.Create(request.Name, request.ShortName, request.Slug, request.SportId,
            request.LogoUrl, request.Country, request.CountryCode);
        context.Teams.Add(team);
        await context.SaveChangesAsync(cancellationToken);
        return team.Id;
    }
}
