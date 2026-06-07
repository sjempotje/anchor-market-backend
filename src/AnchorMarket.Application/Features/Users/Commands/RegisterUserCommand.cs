using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Users.Commands;

public record RegisterUserCommand(string Username, string Email) : IRequest<Guid>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public RegisterUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users.AnyAsync(
            u => u.Username == request.Username || u.Email.ToLower() == request.Email.ToLower(), 
            cancellationToken);

        if (existingUser)
            throw new InvalidOperationException("Username or email already exists.");

        var user = User.Create(request.Username, request.Email);
        user.CreateWallet();

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
