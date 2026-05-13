using MediatR;

namespace AnchorMarket.Application.Features.Users.Commands;

public record RegisterUserCommand(string Username, string Email) : IRequest<Guid>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    public Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
