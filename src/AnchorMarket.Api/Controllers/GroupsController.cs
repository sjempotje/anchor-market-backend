using System;
using AnchorMarket.Application.Features.Groups.Commands;
using AnchorMarket.Application.Features.Groups.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages user groups for collaborative markets.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves all groups.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of groups.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetGroupsQuery(), cancellationToken));

    /// <summary>Retrieves a group by its ID.</summary>
    /// <param name="id">The group ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The group if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        Guid? callerId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed)
            ? parsed
            : null;
        var group = await sender.Send(new GetGroupByIdQuery(id, callerId), cancellationToken);
        return group is null ? NotFound() : Ok(group);
    }

    /// <summary>Creates a new group.</summary>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 response with the new group ID.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>Updates an existing group.</summary>
    /// <param name="id">The group ID.</param>
    /// <param name="command">The update command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupCommand command, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id != command.GroupId) return BadRequest();
        await sender.Send(command with { CallerId = callerId }, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a group by ID.</summary>
    /// <param name="id">The group ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(new DeleteGroupCommand(id, callerId), cancellationToken);
        return NoContent();
    }

    /// <summary>Joins a group as the authenticated user.</summary>
    /// <param name="id">The group ID.</param>
    /// <param name="request">Join request with optional join code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> Join(Guid id, [FromBody] JoinGroupRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(new JoinGroupCommand(id, userId, request.JoinCode), cancellationToken);
        return NoContent();
    }

    /// <summary>Retrieves a group's members. Private groups restrict this to members/owner.</summary>
    /// <param name="id">The group ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The group's memberships.</returns>
    [HttpGet("{id:guid}/members")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken cancellationToken)
    {
        Guid? callerId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed)
            ? parsed
            : null;
        return Ok(await sender.Send(new GetGroupMembersQuery(id, callerId), cancellationToken));
    }

    /// <summary>Leaves a group as the authenticated user.</summary>
    /// <param name="id">The group ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}/leave")]
    public async Task<IActionResult> Leave(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(new LeaveGroupCommand(id, userId), cancellationToken);
        return NoContent();
    }
}

/// <summary>Request model for joining a group.</summary>
public record JoinGroupRequest(string? JoinCode = null);
