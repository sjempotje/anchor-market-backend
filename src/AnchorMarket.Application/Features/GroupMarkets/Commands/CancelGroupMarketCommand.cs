using MediatR;

namespace AnchorMarket.Application.Features.GroupMarkets.Commands;

/// <summary>
/// Cancels an open group market. Only the creator or a group admin may cancel.
/// Business rule enforced in the handler.
/// </summary>
public record CancelGroupMarketCommand(Guid MarketId, Guid RequestingUserId) : IRequest;

public class CancelGroupMarketCommandHandler : IRequestHandler<CancelGroupMarketCommand>
{
    public Task Handle(CancelGroupMarketCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
