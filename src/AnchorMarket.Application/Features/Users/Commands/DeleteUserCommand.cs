using MediatR;

namespace AnchorMarket.Application.Features.Users.Commands;

public record DeleteUserCommand(Guid UserId) : IRequest;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    public Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
