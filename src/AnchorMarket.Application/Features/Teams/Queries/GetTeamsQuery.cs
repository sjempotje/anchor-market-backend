using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Teams.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Teams.Queries;

/// <summary>Query to retrieve all teams.</summary>
public record GetTeamsQuery : IRequest<List<TeamDto>>;

/// <summary>Handles retrieving all teams.</summary>
public class GetTeamsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetAllQueryHandler<Team, GetTeamsQuery, TeamDto>(context, mapper);
