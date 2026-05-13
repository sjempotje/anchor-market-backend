using MediatR;

namespace AnchorMarket.Application.Features.GroupMarkets.Commands;

/// <summary>
/// Resolves a group market. The ResolverId MUST be a group member who is NOT the market creator.
/// This two-person rule is enforced in the handler, not in the controller or database.
/// </summary>
public record ResolveGroupMarketCommand(
    Guid MarketId,
    Guid WinningOutcomeId,
    Guid ResolverId) : IRequest;

public class ResolveGroupMarketCommandHandler : IRequestHandler<ResolveGroupMarketCommand>
{
    public Task Handle(ResolveGroupMarketCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
