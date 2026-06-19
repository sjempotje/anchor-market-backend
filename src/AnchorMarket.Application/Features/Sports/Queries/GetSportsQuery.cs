using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Sports.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Sports.Queries;

/// <summary>Query to retrieve all sports.</summary>
public record GetSportsQuery : IRequest<List<SportDto>>;

/// <summary>Handles retrieving all sports.</summary>
public class GetSportsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetAllQueryHandler<Sport, GetSportsQuery, SportDto>(context, mapper);
