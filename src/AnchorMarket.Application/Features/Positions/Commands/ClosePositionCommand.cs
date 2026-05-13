using MediatR;

namespace AnchorMarket.Application.Features.Positions.Commands;

/// <summary>Closes (sells) a user's open position, returning funds proportionally to their wallet.</summary>
public record ClosePositionCommand(Guid PositionId, Guid UserId) : IRequest;

public class ClosePositionCommandHandler : IRequestHandler<ClosePositionCommand>
{
    public Task Handle(ClosePositionCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
