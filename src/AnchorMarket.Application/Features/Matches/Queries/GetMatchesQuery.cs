using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Matches.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Matches.Queries;

/// <summary>Query to retrieve all matches.</summary>
public record GetMatchesQuery : IRequest<List<MatchDto>>;

/// <summary>Handles retrieving all matches.</summary>
public class GetMatchesQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetAllQueryHandler<Match, GetMatchesQuery, MatchDto>(context, mapper);
