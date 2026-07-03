using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Groups.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Groups.Queries;

/// <summary>Query to retrieve all groups.</summary>
public record GetGroupsQuery : IRequest<List<GroupDto>>;

/// <summary>Handles retrieving all groups.</summary>
public class GetGroupsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetGroupsQuery, List<GroupDto>>
{
    public async Task<List<GroupDto>> Handle(GetGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await context.Groups
            .ProjectTo<GroupDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return groups.Select(g => g with { JoinCode = null }).ToList();
    }
}
