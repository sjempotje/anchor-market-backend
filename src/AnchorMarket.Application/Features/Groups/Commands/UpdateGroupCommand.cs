using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using MediatR;

namespace AnchorMarket.Application.Features.Groups.Commands;

/// <summary>Command to update a group's details.</summary>
public record UpdateGroupCommand(
    Guid GroupId,
    Guid CallerId,
    string Name,
    string? Description) : IRequest;

/// <summary>Handles updating a group.</summary>
public class UpdateGroupCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateGroupCommand>
{
    /// <summary>Updates the group if the caller is the owner.</summary>
    public async Task Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await context.Groups.FindAsync([request.GroupId], cancellationToken)
            ?? throw new NotFoundException($"Group with ID {request.GroupId} not found.");

        if (group.OwnerId != request.CallerId)
            throw new ForbiddenException("Only the group owner can update this group.");

        group.Update(request.Name, request.Description);
        await context.SaveChangesAsync(cancellationToken);
    }
}
