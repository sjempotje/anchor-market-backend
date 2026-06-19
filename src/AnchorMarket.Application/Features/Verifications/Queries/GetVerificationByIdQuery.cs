using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Verifications.DTOs;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Verifications.Queries;

/// <summary>Query to retrieve a verification by its ID.</summary>
public record GetVerificationByIdQuery(Guid Id) : IRequest<VerificationDto?>;

/// <summary>Handles retrieving a verification by ID.</summary>
public class GetVerificationByIdQueryHandler : IRequestHandler<GetVerificationByIdQuery, VerificationDto?>
{
    private readonly IApplicationDbContext _context;

    public GetVerificationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Retrieves the verification by ID, or null if not found.</summary>
    public async Task<VerificationDto?> Handle(GetVerificationByIdQuery request, CancellationToken cancellationToken)
    {
        var verification = await _context.Set<Verification>().FindAsync([request.Id], cancellationToken);

        if (verification is null)
            return null;

        return new VerificationDto(
            verification.Id,
            verification.Identifier,
            verification.Value,
            verification.ExpiresAt,
            verification.CreatedAt,
            verification.UpdatedAt);
    }
}
