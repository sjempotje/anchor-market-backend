using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Groups.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Groups.Queries;

/// <summary>Query to retrieve a group by its ID.</summary>
/// <param name="Id">The group ID.</param>
/// <param name="CallerId">
/// The authenticated caller, if any. Only the owner or a member may see the group's join code.
/// </param>
public record GetGroupByIdQuery(Guid Id, Guid? CallerId = null) : IRequest<GroupDto?>;

/// <summary>Handles retrieving a group by ID, redacting the join code from non-members.</summary>
public class GetGroupByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetGroupByIdQuery, GroupDto?>
{
    public async Task<GroupDto?> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var group = await context.Groups
            .Where(g => g.Id == request.Id)
            .ProjectTo<GroupDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (group is null) return null;
        if (group.JoinCode is null) return group;

        var canSeeJoinCode = request.CallerId is { } callerId &&
            (callerId == group.OwnerId ||
             await context.GroupMemberships.AnyAsync(
                 m => m.GroupId == request.Id && m.UserId == callerId, cancellationToken));

        return canSeeJoinCode ? group : group with { JoinCode = null };
    }
}
