using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Teams.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Teams.Queries;

/// <summary>Query to retrieve a team by its ID.</summary>
public record GetTeamByIdQuery(Guid Id) : IRequest<TeamDto?>, IGetByIdQuery;

/// <summary>Handles retrieving a team by ID.</summary>
public class GetTeamByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Team, GetTeamByIdQuery, TeamDto>(context, mapper);
