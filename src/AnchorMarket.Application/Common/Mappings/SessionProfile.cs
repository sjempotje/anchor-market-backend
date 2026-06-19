using AnchorMarket.Application.Features.Sessions.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Session"/> and <see cref="SessionDto"/>.</summary>
public class SessionProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public SessionProfile()
    {
        CreateMap<Session, SessionDto>();
    }
}
