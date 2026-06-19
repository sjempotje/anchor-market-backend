using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using MediatR;

namespace AnchorMarket.Application.Features.Notifications.Commands;

/// <summary>Command to mark a notification as read.</summary>
public record MarkNotificationReadCommand(Guid Id, Guid UserId) : IRequest;

/// <summary>Handles marking a notification as read.</summary>
public class MarkNotificationReadCommandHandler(IApplicationDbContext context)
    : IRequestHandler<MarkNotificationReadCommand>
{
    /// <summary>Marks the notification as read if owned by the user.</summary>
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await context.Notifications.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException($"Notification {request.Id} not found.");

        if (notification.UserId != request.UserId)
            throw new ForbiddenException("You do not have access to this notification.");

        notification.MarkAsRead();
        await context.SaveChangesAsync(cancellationToken);
    }
}
