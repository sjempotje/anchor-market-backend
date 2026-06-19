using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Features.Notifications.DTOs;

/// <summary>Data transfer object for a notification.</summary>
public record NotificationDto(
    Guid Id,
    Guid UserId,
    NotificationType Type,
    string Title,
    string Body,
    bool IsRead,
    Guid? RelatedEntityId,
    DateTimeOffset CreatedAt);
