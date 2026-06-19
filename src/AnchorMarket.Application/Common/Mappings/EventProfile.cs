using AnchorMarket.Application.Features.Events.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Event"/> and <see cref="EventDto"/>.</summary>
public class EventProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public EventProfile()
    {
        CreateMap<Event, EventDto>();
    }
}
