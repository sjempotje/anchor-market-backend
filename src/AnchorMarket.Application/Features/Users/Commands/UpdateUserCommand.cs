using MediatR;

namespace AnchorMarket.Application.Features.Users.Commands;

public record UpdateUserCommand(Guid UserId, string Username, string Email) : IRequest;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    public Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
