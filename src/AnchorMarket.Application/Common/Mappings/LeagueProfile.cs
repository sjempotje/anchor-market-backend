using AnchorMarket.Application.Features.Leagues.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="League"/> and <see cref="LeagueDto"/>.</summary>
public class LeagueProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public LeagueProfile()
    {
        CreateMap<League, LeagueDto>();
    }
}
