using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Groups.Commands;

/// <summary>Command for a user to join a group.</summary>
public record JoinGroupCommand(Guid GroupId, Guid UserId) : IRequest;

/// <summary>Handles joining a group.</summary>
public class JoinGroupCommandHandler : IRequestHandler<JoinGroupCommand>
{
    private readonly IApplicationDbContext _context;

    public JoinGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(JoinGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.Groups.FindAsync([request.GroupId], cancellationToken)
            ?? throw new KeyNotFoundException($"Group {request.GroupId} not found.");

        var alreadyMember = await _context.GroupMemberships
            .AnyAsync(m => m.GroupId == request.GroupId && m.UserId == request.UserId, cancellationToken);

        if (!alreadyMember)
        {
            var membership = GroupMembership.Create(request.UserId, request.GroupId);
            _context.GroupMemberships.Add(membership);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
