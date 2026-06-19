using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System.Text.Json.Serialization;

namespace AnchorMarket.Application.Features.Users.Commands;

/// <summary>Command to update a user's profile. Caller must own the account.</summary>
public record UpdateUserCommand(Guid UserId, string Username, string Email, [property: JsonIgnore] Guid CallerId = default) : IRequest;

/// <summary>Handles updating a user's profile.</summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Updates the user's username and email if not already taken.</summary>
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != request.CallerId)
            throw new ForbiddenException("You can only update your own profile.");

        var user = await _context.Users.FindAsync([request.UserId], cancellationToken);

        if (user is null)
            throw new NotFoundException($"User with ID {request.UserId} not found.");

        var existingUser = await _context.Users.AnyAsync(
            u => u.Id != request.UserId &&
                 (u.Username == request.Username || u.Email.ToLower() == request.Email.ToLower()),
            cancellationToken);

        if (existingUser)
            throw new InvalidOperationException("Username or email already exists.");

        user.Update(request.Username, request.Email);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
