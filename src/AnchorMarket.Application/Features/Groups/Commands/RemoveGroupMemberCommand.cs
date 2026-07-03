using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Groups.Commands;

/// <summary>Command for the group owner to remove a member from a group.</summary>
public record RemoveGroupMemberCommand(Guid GroupId, Guid CallerId, Guid TargetUserId) : IRequest;

/// <summary>Handles removing a member from a group.</summary>
public class RemoveGroupMemberCommandHandler(IApplicationDbContext context) : IRequestHandler<RemoveGroupMemberCommand>
{
    /// <summary>Removes the target member if the caller is the group owner.</summary>
    public async Task Handle(RemoveGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var group = await context.Groups.FindAsync([request.GroupId], cancellationToken)
            ?? throw new NotFoundException($"Group with ID {request.GroupId} not found.");

        if (group.OwnerId != request.CallerId)
            throw new ForbiddenException("Only the group owner can remove members.");

        if (request.TargetUserId == group.OwnerId)
            throw new ForbiddenException("The group owner cannot be removed.");

        var membership = await context.GroupMemberships
            .FirstOrDefaultAsync(m => m.GroupId == request.GroupId && m.UserId == request.TargetUserId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {request.TargetUserId} is not a member of this group.");

        context.GroupMemberships.Remove(membership);
        await context.SaveChangesAsync(cancellationToken);
    }
}
