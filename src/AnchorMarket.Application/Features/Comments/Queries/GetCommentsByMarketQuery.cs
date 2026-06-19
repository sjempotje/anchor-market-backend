using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Comments.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Comments.Queries;

/// <summary>Query to retrieve all comments for a market.</summary>
public record GetCommentsByMarketQuery(Guid MarketId) : IRequest<List<CommentDto>>;

/// <summary>Handles retrieving comments by market.</summary>
public class GetCommentsByMarketQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetCommentsByMarketQuery, List<CommentDto>>
{
    /// <summary>Returns the comments for the specified market, ordered by creation date.</summary>
    public async Task<List<CommentDto>> Handle(GetCommentsByMarketQuery request, CancellationToken cancellationToken)
    {
        var comments = await context.Comments
            .Where(c => c.MarketId == request.MarketId)
            .ProjectTo<CommentDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return [.. comments.OrderBy(c => c.CreatedAt)];
    }
}
