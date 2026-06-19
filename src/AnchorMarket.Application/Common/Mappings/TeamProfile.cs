using AnchorMarket.Application.Features.Teams.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Team"/> and <see cref="TeamDto"/>.</summary>
public class TeamProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public TeamProfile()
    {
        CreateMap<Team, TeamDto>();
    }
}
