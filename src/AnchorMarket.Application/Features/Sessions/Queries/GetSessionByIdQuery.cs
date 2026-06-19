using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Sessions.DTOs;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Sessions.Queries;

/// <summary>Query to retrieve a session by its ID.</summary>
public record GetSessionByIdQuery(Guid Id) : IRequest<SessionDto?>;

/// <summary>Handles retrieving a session by ID.</summary>
public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, SessionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSessionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Retrieves the session by ID, or null if not found.</summary>
    public async Task<SessionDto?> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _context.Set<Session>().FindAsync([request.Id], cancellationToken);

        if (session is null)
            return null;

        return new SessionDto(
            session.Id,
            session.UserId,
            session.Token,
            session.ExpiresAt,
            session.IpAddress,
            session.UserAgent,
            session.CreatedAt,
            session.UpdatedAt);
    }
}
