using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Positions.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Positions.Queries;

/// <summary>Query to retrieve positions belonging to the authenticated user.</summary>
public record GetPositionsQuery(Guid UserId) : IRequest<List<PositionDto>>;

/// <summary>Handles retrieving positions for a specific user.</summary>
public class GetPositionsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetPositionsQuery, List<PositionDto>>
{
    public Task<List<PositionDto>> Handle(GetPositionsQuery request, CancellationToken cancellationToken)
        => context.Positions
            .Where(p => p.UserId == request.UserId)
            .ProjectTo<PositionDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}
