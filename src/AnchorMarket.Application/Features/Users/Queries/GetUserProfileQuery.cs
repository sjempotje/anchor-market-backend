using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Users.DTOs;
using MediatR;

namespace AnchorMarket.Application.Features.Users.Queries;

/// <summary>Query to retrieve a user's public profile.</summary>
public record GetUserProfileQuery(Guid UserId) : IRequest<UserDto?>;

/// <summary>Handles retrieving a user's profile.</summary>
public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserDto?>
{
    private readonly IApplicationDbContext _context;

    public GetUserProfileQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Retrieves the user profile by user ID, or null if not found.</summary>
    public async Task<UserDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync([request.UserId], cancellationToken);

        if (user is null)
            return null;

        return new UserDto(
            user.Id,
            user.Username,
            user.Name,
            user.Image,
            user.Bio,
            user.IsVerifiedCreator,
            user.FollowersCount,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
