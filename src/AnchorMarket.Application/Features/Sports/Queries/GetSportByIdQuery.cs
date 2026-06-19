using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Sports.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Sports.Queries;

/// <summary>Query to retrieve a sport by its ID.</summary>
public record GetSportByIdQuery(Guid Id) : IRequest<SportDto?>, IGetByIdQuery;

/// <summary>Handles retrieving a sport by ID.</summary>
public class GetSportByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Sport, GetSportByIdQuery, SportDto>(context, mapper);
