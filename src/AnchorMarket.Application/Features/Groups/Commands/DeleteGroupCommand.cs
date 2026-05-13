using MediatR;

namespace AnchorMarket.Application.Features.Groups.Commands;

public record DeleteGroupCommand(Guid GroupId) : IRequest;

public class DeleteGroupCommandHandler : IRequestHandler<DeleteGroupCommand>
{
    public Task Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
