using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Common.Queries;

/// <summary>Marker interface for query records that carry a single entity ID.</summary>
public interface IGetByIdQuery
{
    /// <summary>Gets the entity ID.</summary>
    Guid Id { get; }
}

/// <summary>Abstract base handler for single-entity queries that projects the matched entity to a DTO.</summary>
public abstract class GetByIdQueryHandler<TEntity, TRequest, TDto>
    : IRequestHandler<TRequest, TDto?>
    where TEntity : BaseEntity
    where TRequest : IRequest<TDto?>, IGetByIdQuery
    where TDto : class
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    /// <summary>Initializes the handler with the database context and AutoMapper.</summary>
    /// <param name="context">The application database context.</param>
    /// <param name="mapper">AutoMapper instance.</param>
    protected GetByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>Projects the entity matching the request ID to a DTO, or null if not found.</summary>
    /// <param name="request">The query request containing the entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The DTO if found, otherwise null.</returns>
    public Task<TDto?> Handle(TRequest request, CancellationToken cancellationToken)
        => _context.Set<TEntity>()
            .Where(e => e.Id == request.Id)
            .ProjectTo<TDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
}
