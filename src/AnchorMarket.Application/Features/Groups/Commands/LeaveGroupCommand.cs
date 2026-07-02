using AnchorMarket.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Groups.Commands;

/// <summary>Command for a user to leave a group.</summary>
public record LeaveGroupCommand(Guid GroupId, Guid UserId) : IRequest;

/// <summary>Handles leaving a group.</summary>
public class LeaveGroupCommandHandler : IRequestHandler<LeaveGroupCommand>
{
    private readonly IApplicationDbContext _context;

    public LeaveGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(LeaveGroupCommand request, CancellationToken cancellationToken)
    {
        var membership = await _context.GroupMemberships
            .FirstOrDefaultAsync(m => m.GroupId == request.GroupId && m.UserId == request.UserId, cancellationToken);

        if (membership is not null)
        {
            _context.GroupMemberships.Remove(membership);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
