using AnchorMarket.Application.Features.Users.DTOs;
using MediatR;

namespace AnchorMarket.Application.Features.Users.Queries;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserDto?>;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserDto?>
{
    public Task<UserDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
