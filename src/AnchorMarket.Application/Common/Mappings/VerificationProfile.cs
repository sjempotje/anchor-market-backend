using AnchorMarket.Application.Features.Verifications.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

public class VerificationProfile : Profile
{
    public VerificationProfile()
    {
        CreateMap<Verification, VerificationDto>();
    }
}
