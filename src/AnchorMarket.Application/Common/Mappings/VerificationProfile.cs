using AnchorMarket.Application.Features.Verifications.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Verification"/> and <see cref="VerificationDto"/>.</summary>
public class VerificationProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public VerificationProfile()
    {
        CreateMap<Verification, VerificationDto>();
    }
}
