using AnchorMarket.Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Common.Queries;

/// <summary>
/// Abstract base handler for "get all" queries.
///
///   public record GetMarketsQuery : IRequest List&lt;MarketDto&gt;&gt;;
///   public class GetMarketsQueryHandler(IApplicationDbContext ctx, IMapper mapper)
///       : GetAllQueryHandler&lt;Market, GetMarketsQuery, MarketDto&gt;(ctx, mapper);
///
/// </summary>
public abstract class GetAllQueryHandler<TEntity, TRequest, TDto>
    : IRequestHandler<TRequest, List<TDto>>
    where TEntity : class
    where TRequest : IRequest<List<TDto>>
    where TDto : class
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    protected GetAllQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task<List<TDto>> Handle(TRequest request, CancellationToken cancellationToken)
        => _context.Set<TEntity>()
            .ProjectTo<TDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}
