using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Common.Queries;

/// <summary>Marker interface, implemented by query records that carry a single Id.</summary>
public interface IGetByIdQuery
{
    Guid Id { get; }
}

/// <summary>
/// Abstract base handler for "get by id" queries.
/// Define a query record and extend this class to get a working handler with zero boilerplate:
///
///   public record GetMarketByIdQuery(Guid Id) : IRequest&lt;MarketDto?&gt;, IGetByIdQuery;
///   public class GetMarketByIdQueryHandler(IApplicationDbContext ctx, IMapper mapper)
///       : GetByIdQueryHandler&lt;Market, GetMarketByIdQuery, MarketDto&gt;(ctx, mapper);
///
/// MediatR discovers the concrete handler automatically, no DI registration needed.
/// </summary>
public abstract class GetByIdQueryHandler<TEntity, TRequest, TDto>
    : IRequestHandler<TRequest, TDto?>
    where TEntity : BaseEntity
    where TRequest : IRequest<TDto?>, IGetByIdQuery
    where TDto : class
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    protected GetByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task<TDto?> Handle(TRequest request, CancellationToken cancellationToken)
        => _context.Set<TEntity>()
            .Where(e => e.Id == request.Id)
            .ProjectTo<TDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
}
