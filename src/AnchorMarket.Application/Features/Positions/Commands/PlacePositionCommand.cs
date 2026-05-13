using MediatR;

namespace AnchorMarket.Application.Features.Positions.Commands;

/// <summary>Place a bet on an outcome. Debits the user's wallet by Amount.</summary>
public record PlacePositionCommand(
    Guid UserId,
    Guid OutcomeId,
    decimal Amount) : IRequest<Guid>;

public class PlacePositionCommandHandler : IRequestHandler<PlacePositionCommand, Guid>
{
    public Task<Guid> Handle(PlacePositionCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
