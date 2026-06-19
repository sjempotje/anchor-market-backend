using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Positions.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Positions.Queries;

/// <summary>Query to retrieve a position by its ID.</summary>
public record GetPositionByIdQuery(Guid Id, Guid CallerId) : IRequest<PositionDto?>;

/// <summary>Handles retrieving a position by ID.</summary>
public class GetPositionByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetPositionByIdQuery, PositionDto?>
{
    /// <summary>Returns the position if owned by the caller.</summary>
    public async Task<PositionDto?> Handle(GetPositionByIdQuery request, CancellationToken cancellationToken)
    {
        var position = await context.Positions
            .Where(p => p.Id == request.Id)
            .ProjectTo<PositionDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (position is null) return null;
        if (position.UserId != request.CallerId) throw new ForbiddenException("You do not own this position.");
        return position;
    }
}
