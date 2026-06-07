using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Positions.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Positions.Queries;

public record GetPositionsByMarketQuery(Guid MarketId, Guid UserId) : IRequest<List<PositionDto>>;

public class GetPositionsByMarketQueryHandler : IRequestHandler<GetPositionsByMarketQuery, List<PositionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPositionsByMarketQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PositionDto>> Handle(GetPositionsByMarketQuery request, CancellationToken cancellationToken)
    {
        var positions = await _context.Positions
            .Where(p => p.Outcome.MarketId == request.MarketId && p.UserId == request.UserId)
            .ProjectTo<PositionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return positions;
    }
}
