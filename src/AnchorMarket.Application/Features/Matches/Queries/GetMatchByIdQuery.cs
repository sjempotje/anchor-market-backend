using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Matches.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Matches.Queries;

/// <summary>Query to retrieve a match by its ID.</summary>
public record GetMatchByIdQuery(Guid Id) : IRequest<MatchDto?>, IGetByIdQuery;

/// <summary>Handles retrieving a match by ID.</summary>
public class GetMatchByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Match, GetMatchByIdQuery, MatchDto>(context, mapper);
