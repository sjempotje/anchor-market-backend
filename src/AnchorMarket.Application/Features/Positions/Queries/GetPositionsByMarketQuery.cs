using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Positions.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Positions.Queries;

/// <summary>Query to retrieve positions for a user in a specific market.</summary>
public record GetPositionsByMarketQuery(Guid MarketId, Guid UserId) : IRequest<List<PositionDto>>;

/// <summary>Handles retrieving positions by market.</summary>
public class GetPositionsByMarketQueryHandler : IRequestHandler<GetPositionsByMarketQuery, List<PositionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPositionsByMarketQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>Returns the user's positions for the specified market.</summary>
    public async Task<List<PositionDto>> Handle(GetPositionsByMarketQuery request, CancellationToken cancellationToken)
    {
        var positions = await _context.Positions
            .Where(p => p.Outcome.MarketId == request.MarketId && p.UserId == request.UserId)
            .ProjectTo<PositionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return positions;
    }
}
