using AnchorMarket.Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Common.Queries;

/// <summary>Abstract base handler for list queries that projects all entities to DTOs via AutoMapper.</summary>
public abstract class GetAllQueryHandler<TEntity, TRequest, TDto>
    : IRequestHandler<TRequest, List<TDto>>
    where TEntity : class
    where TRequest : IRequest<List<TDto>>
    where TDto : class
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    /// <summary>Initializes the handler with the database context and AutoMapper.</summary>
    /// <param name="context">The application database context.</param>
    /// <param name="mapper">AutoMapper instance.</param>
    protected GetAllQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>Projects all entities to DTOs and returns them as a list.</summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of DTOs representing all entities.</returns>
    public Task<List<TDto>> Handle(TRequest request, CancellationToken cancellationToken)
        => _context.Set<TEntity>()
            .ProjectTo<TDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}
