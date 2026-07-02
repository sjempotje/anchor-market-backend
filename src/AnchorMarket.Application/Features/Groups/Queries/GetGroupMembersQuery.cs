using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Groups.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Groups.Queries;

/// <summary>Query to retrieve a group's members.</summary>
/// <param name="GroupId">The group ID.</param>
/// <param name="CallerId">The authenticated caller, if any. Required for private groups.</param>
public record GetGroupMembersQuery(Guid GroupId, Guid? CallerId = null) : IRequest<List<GroupMembershipDto>>;

/// <summary>Handles retrieving a group's members, restricting private groups to members/owner.</summary>
public class GetGroupMembersQueryHandler(IApplicationDbContext context) : IRequestHandler<GetGroupMembersQuery, List<GroupMembershipDto>>
{
    public async Task<List<GroupMembershipDto>> Handle(GetGroupMembersQuery request, CancellationToken cancellationToken)
    {
        var group = await context.Groups
            .Where(g => g.Id == request.GroupId)
            .Select(g => new { g.IsPrivate, g.OwnerId })
            .FirstOrDefaultAsync(cancellationToken);

        if (group is null)
            throw new NotFoundException($"Group with ID {request.GroupId} not found.");

        if (group.IsPrivate)
        {
            var isMember = request.CallerId is { } callerId &&
                (callerId == group.OwnerId ||
                 await context.GroupMemberships.AnyAsync(
                     m => m.GroupId == request.GroupId && m.UserId == callerId, cancellationToken));

            if (!isMember)
                throw new ForbiddenException("You are not a member of this group.");
        }

        return await context.GroupMemberships
            .Where(m => m.GroupId == request.GroupId)
            .OrderBy(m => m.JoinedAt)
            .Select(m => new GroupMembershipDto(m.Id, m.GroupId, m.UserId, m.JoinedAt))
            .ToListAsync(cancellationToken);
    }
}
