using AnchorMarket.Application.Features.Sports.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Sport"/> and <see cref="SportDto"/>.</summary>
public class SportProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public SportProfile()
    {
        CreateMap<Sport, SportDto>();
    }
}
