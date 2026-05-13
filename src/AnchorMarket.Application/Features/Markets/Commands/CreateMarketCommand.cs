using AnchorMarket.Domain.Enums;
using MediatR;

namespace AnchorMarket.Application.Features.Markets.Commands;

public record CreateMarketCommand(
    string Title,
    string Description,
    DateTimeOffset ResolutionDeadline,
    MarketScope Scope,
    Guid CreatorId,
    Guid? GroupId,
    IReadOnlyList<string> OutcomeTitles) : IRequest<Guid>;

public class CreateMarketCommandHandler : IRequestHandler<CreateMarketCommand, Guid>
{
    public Task<Guid> Handle(CreateMarketCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
