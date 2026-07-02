using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Groups.Commands;

/// <summary>Command for a user to join a group.</summary>
/// <param name="GroupId">The group to join.</param>
/// <param name="UserId">The user joining.</param>
/// <param name="JoinCode">Optional join code required for private groups.</param>
public record JoinGroupCommand(Guid GroupId, Guid UserId, string? JoinCode = null) : IRequest;

/// <summary>Handles joining a group with privacy checks.</summary>
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

        if (group.IsPrivate && group.JoinCode != request.JoinCode)
            throw new InvalidOperationException("Invalid or missing join code for private group.");

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
