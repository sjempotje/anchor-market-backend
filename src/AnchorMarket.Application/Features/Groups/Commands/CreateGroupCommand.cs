using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Groups.Commands;

/// <summary>Command to create a new group.</summary>
public record CreateGroupCommand(
    string Name,
    string? Description,
    Guid OwnerId) : IRequest<Guid>;

/// <summary>Handles the creation of a group.</summary>
public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Creates the group and returns its ID.</summary>
    public async Task<Guid> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = Group.Create(request.Name, request.Description, request.OwnerId);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
        return group.Id;
    }
}
