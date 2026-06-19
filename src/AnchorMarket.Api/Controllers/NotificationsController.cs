using AnchorMarket.Application.Features.Notifications.Commands;
using AnchorMarket.Application.Features.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages user notifications.</summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves notifications for the authenticated user.</summary>
    /// <param name="unreadOnly">If true, returns only unread notifications.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of notifications.</returns>
    [HttpGet]
    public async Task<IActionResult> GetByUser([FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await sender.Send(new GetNotificationsByUserQuery(userId, unreadOnly), cancellationToken));
    }

    /// <summary>Marks a notification as read.</summary>
    /// <param name="id">The notification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(new MarkNotificationReadCommand(id, userId), cancellationToken);
        return NoContent();
    }
}
