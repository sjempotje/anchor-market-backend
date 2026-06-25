using AnchorMarket.Application.Features.ExternalFeeds.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping feed entities to their DTOs.</summary>
public class FeedRegistrationProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings for feeds. AuthToken is intentionally not exposed.</summary>
    public FeedRegistrationProfile()
    {
        CreateMap<ExternalFeedRegistration, FeedRegistrationDto>();
        CreateMap<FeedResult, FeedResultDto>();
    }
}
