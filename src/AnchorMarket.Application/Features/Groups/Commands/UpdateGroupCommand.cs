using MediatR;

namespace AnchorMarket.Application.Features.Groups.Commands;

public record UpdateGroupCommand(
    Guid GroupId,
    string Name,
    string? Description) : IRequest;

public class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand>
{
    public Task Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
