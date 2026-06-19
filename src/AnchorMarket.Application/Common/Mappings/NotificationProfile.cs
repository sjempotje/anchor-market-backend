using AnchorMarket.Application.Features.Notifications.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Notification"/> and <see cref="NotificationDto"/>.</summary>
public class NotificationProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public NotificationProfile()
    {
        CreateMap<Notification, NotificationDto>();
    }
}
