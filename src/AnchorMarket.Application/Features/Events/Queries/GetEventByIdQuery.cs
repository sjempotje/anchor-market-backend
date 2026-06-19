using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Events.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Events.Queries;

/// <summary>Query to retrieve an event by its ID.</summary>
public record GetEventByIdQuery(Guid Id) : IRequest<EventDto?>, IGetByIdQuery;

/// <summary>Handles retrieving an event by ID.</summary>
public class GetEventByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Event, GetEventByIdQuery, EventDto>(context, mapper);
