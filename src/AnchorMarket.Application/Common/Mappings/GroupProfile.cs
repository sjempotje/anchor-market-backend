using AnchorMarket.Application.Features.Groups.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Group"/> and <see cref="GroupDto"/>.</summary>
public class GroupProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public GroupProfile()
    {
        CreateMap<Group, GroupDto>();
    }
}
