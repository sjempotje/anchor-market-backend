using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Groups.Commands;

public record DeleteGroupCommand(Guid GroupId) : IRequest;

public class DeleteGroupCommandHandler : IRequestHandler<DeleteGroupCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.Groups.FindAsync([request.GroupId], cancellationToken);

        if (group is null)
            throw new NotFoundException($"Group with ID {request.GroupId} not found.");

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
