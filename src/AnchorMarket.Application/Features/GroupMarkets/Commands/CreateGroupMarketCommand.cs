using MediatR;

namespace AnchorMarket.Application.Features.GroupMarkets.Commands;

public record CreateGroupMarketCommand(
    Guid GroupId,
    Guid CreatorId,
    string Title,
    string Description,
    DateTimeOffset ResolutionDeadline,
    IReadOnlyList<string> OutcomeTitles) : IRequest<Guid>;

public class CreateGroupMarketCommandHandler : IRequestHandler<CreateGroupMarketCommand, Guid>
{
    public Task<Guid> Handle(CreateGroupMarketCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
