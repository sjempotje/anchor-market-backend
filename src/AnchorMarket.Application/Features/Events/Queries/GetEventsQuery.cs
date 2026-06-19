using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Events.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Events.Queries;

/// <summary>Query to retrieve all events.</summary>
public record GetEventsQuery : IRequest<List<EventDto>>;

/// <summary>Handles retrieving all events.</summary>
public class GetEventsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetAllQueryHandler<Event, GetEventsQuery, EventDto>(context, mapper);
