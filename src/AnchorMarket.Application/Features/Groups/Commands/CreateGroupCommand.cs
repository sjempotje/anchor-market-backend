using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Groups.Commands;

/// <summary>Command to create a new group.</summary>
public record CreateGroupCommand(
    string Name,
    string? Description,
    Guid OwnerId,
    bool IsPrivate = false) : IRequest<Guid>;

/// <summary>Handles the creation of a group.</summary>
public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Creates the group, adds the owner as a member, and returns the group's ID.</summary>
    public async Task<Guid> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = Group.Create(request.Name, request.Description, request.OwnerId, request.IsPrivate);
        _context.Groups.Add(group);

        var ownerMembership = GroupMembership.Create(request.OwnerId, group.Id);
        _context.GroupMemberships.Add(ownerMembership);

        await _context.SaveChangesAsync(cancellationToken);
        return group.Id;
    }
}
