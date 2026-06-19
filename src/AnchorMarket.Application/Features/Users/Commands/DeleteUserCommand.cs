using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Users.Commands;

/// <summary>Command to delete a user. Caller must be deleting their own account.</summary>
public record DeleteUserCommand(Guid UserId, Guid CallerId) : IRequest;

/// <summary>Handles the deletion of a user.</summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Deletes the user if the caller owns the account.</summary>
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != request.CallerId)
            throw new ForbiddenException("You can only delete your own account.");

        var user = await _context.Users.FindAsync([request.UserId], cancellationToken);

        if (user is null)
            throw new NotFoundException($"User with ID {request.UserId} not found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
