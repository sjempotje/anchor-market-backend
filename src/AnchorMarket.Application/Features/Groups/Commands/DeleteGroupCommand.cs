using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using MediatR;

namespace AnchorMarket.Application.Features.Groups.Commands;

/// <summary>Command to delete a group.</summary>
public record DeleteGroupCommand(Guid GroupId, Guid CallerId) : IRequest;

/// <summary>Handles the deletion of a group.</summary>
public class DeleteGroupCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteGroupCommand>
{
    /// <summary>Deletes the group if the caller is the owner.</summary>
    public async Task Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await context.Groups.FindAsync([request.GroupId], cancellationToken)
            ?? throw new NotFoundException($"Group with ID {request.GroupId} not found.");

        if (group.OwnerId != request.CallerId)
            throw new ForbiddenException("Only the group owner can delete this group.");

        context.Groups.Remove(group);
        await context.SaveChangesAsync(cancellationToken);
    }
}
