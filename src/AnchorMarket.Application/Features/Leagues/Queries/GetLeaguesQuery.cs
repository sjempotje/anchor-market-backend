using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Leagues.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Leagues.Queries;

/// <summary>Query to retrieve all leagues.</summary>
public record GetLeaguesQuery : IRequest<List<LeagueDto>>;

/// <summary>Handles retrieving all leagues.</summary>
public class GetLeaguesQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetAllQueryHandler<League, GetLeaguesQuery, LeagueDto>(context, mapper);
