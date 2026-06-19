using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }

    /// <summary>Optional FK to the entity this notification is about (market, match, order).</summary>
    public Guid? RelatedEntityId { get; private set; }

    public static Notification Create(Guid userId, NotificationType type, string title, string body,
        Guid? relatedEntityId = null)
    {
        return new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            RelatedEntityId = relatedEntityId
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
