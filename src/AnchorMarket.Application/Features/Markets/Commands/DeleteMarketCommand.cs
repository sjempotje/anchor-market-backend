using MediatR;

namespace AnchorMarket.Application.Features.Markets.Commands;

public record DeleteMarketCommand(Guid MarketId) : IRequest;

public class DeleteMarketCommandHandler : IRequestHandler<DeleteMarketCommand>
{
    public Task Handle(DeleteMarketCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
