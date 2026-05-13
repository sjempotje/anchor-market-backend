using MediatR;

namespace AnchorMarket.Application.Features.Groups.Commands;

public record CreateGroupCommand(
    string Name,
    string? Description,
    Guid OwnerId) : IRequest<Guid>;

public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, Guid>
{
    public Task<Guid> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
