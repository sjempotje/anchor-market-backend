using MediatR;

namespace AnchorMarket.Application.Features.Markets.Commands;

public record UpdateMarketCommand(
    Guid MarketId,
    string Title,
    string Description,
    DateTimeOffset ResolutionDeadline) : IRequest;

public class UpdateMarketCommandHandler : IRequestHandler<UpdateMarketCommand>
{
    public Task Handle(UpdateMarketCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
