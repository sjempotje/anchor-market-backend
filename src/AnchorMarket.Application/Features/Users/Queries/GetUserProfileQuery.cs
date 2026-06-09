using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Users.DTOs;
using MediatR;

namespace AnchorMarket.Application.Features.Users.Queries;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserDto?>;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserDto?>
{
    private readonly IApplicationDbContext _context;

    public GetUserProfileQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync([request.UserId], cancellationToken);

        if (user is null)
            return null;

        return new UserDto(
            user.Id,
            user.Username,
            user.Name,
            user.Email,
            user.EmailVerified,
            user.Image,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
