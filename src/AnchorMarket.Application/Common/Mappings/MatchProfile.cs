using AnchorMarket.Application.Features.Matches.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Match"/> and <see cref="MatchDto"/>.</summary>
public class MatchProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public MatchProfile()
    {
        CreateMap<Match, MatchDto>();
        CreateMap<MatchState, MatchStateDto>();
    }
}
