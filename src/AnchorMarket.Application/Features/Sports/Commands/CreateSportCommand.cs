using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using MediatR;

namespace AnchorMarket.Application.Features.Sports.Commands;

/// <summary>Command to create a new sport.</summary>
public record CreateSportCommand(string Name, string Slug, SportType Type, string? Icon = null) : IRequest<Guid>;

/// <summary>Handles the creation of a sport.</summary>
public class CreateSportCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateSportCommand, Guid>
{
    /// <summary>Creates the sport and returns its ID.</summary>
    public async Task<Guid> Handle(CreateSportCommand request, CancellationToken cancellationToken)
    {
        var sport = Sport.Create(request.Name, request.Slug, request.Type, request.Icon);
        context.Sports.Add(sport);
        await context.SaveChangesAsync(cancellationToken);
        return sport.Id;
    }
}
