using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Groups.Commands;

public record UpdateGroupCommand(
    Guid GroupId,
    string Name,
    string? Description) : IRequest;

public class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.Groups.FindAsync([request.GroupId], cancellationToken);

        if (group is null)
            throw new NotFoundException($"Group with ID {request.GroupId} not found.");

        group.Update(request.Name, request.Description);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
