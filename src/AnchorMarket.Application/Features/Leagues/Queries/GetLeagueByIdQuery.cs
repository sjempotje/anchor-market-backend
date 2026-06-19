using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Leagues.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Leagues.Queries;

/// <summary>Query to retrieve a league by its ID.</summary>
public record GetLeagueByIdQuery(Guid Id) : IRequest<LeagueDto?>, IGetByIdQuery;

/// <summary>Handles retrieving a league by ID.</summary>
public class GetLeagueByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<League, GetLeagueByIdQuery, LeagueDto>(context, mapper);
