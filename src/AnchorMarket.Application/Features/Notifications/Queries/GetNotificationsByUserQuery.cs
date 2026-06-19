using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Notifications.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Notifications.Queries;

/// <summary>Query to retrieve notifications for a user.</summary>
public record GetNotificationsByUserQuery(Guid UserId, bool UnreadOnly = false) : IRequest<List<NotificationDto>>;

/// <summary>Handles retrieving notifications for a user.</summary>
public class GetNotificationsByUserQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetNotificationsByUserQuery, List<NotificationDto>>
{
    /// <summary>Returns the user's notifications, optionally filtered to unread only.</summary>
    public Task<List<NotificationDto>> Handle(GetNotificationsByUserQuery request, CancellationToken cancellationToken)
    {
        var query = context.Notifications.Where(n => n.UserId == request.UserId);
        if (request.UnreadOnly)
            query = query.Where(n => !n.IsRead);
        return query
            .OrderByDescending(n => n.CreatedAt)
            .ProjectTo<NotificationDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
