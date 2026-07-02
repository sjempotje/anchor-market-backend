using System;
using AnchorMarket.Application.Features.GroupMarkets.Commands;
using AnchorMarket.Application.Features.GroupMarkets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages group markets for collaborative trading.</summary>
[ApiController]
[Route("api/group-markets")]
[Authorize]
public class GroupMarketsController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves a group market by its ID.</summary>
    /// <param name="id">The group market ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The group market if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var market = await sender.Send(new GetGroupMarketByIdQuery(id, callerId), cancellationToken);
        return market is null ? NotFound() : Ok(market);
    }

    /// <summary>Retrieves group markets for a specific group.</summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of group markets.</returns>
    [HttpGet]
    public async Task<IActionResult> GetByGroup([FromQuery] Guid groupId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await sender.Send(new GetGroupMarketsQuery(groupId, userId), cancellationToken));
    }

    /// <summary>Creates a new group market.</summary>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 response with the new group market ID.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupMarketCommand command, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var id = await sender.Send(command with { CreatorId = callerId }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>Resolves a group market to determine its outcome.</summary>
    /// <param name="id">The group market ID.</param>
    /// <param name="command">The resolve command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveGroupMarketCommand command, CancellationToken cancellationToken)
    {
        if (id != command.MarketId) return BadRequest();
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(command with { ResolverId = callerId }, cancellationToken);
        return NoContent();
    }

    /// <summary>Cancels a group market.</summary>
    /// <param name="id">The group market ID.</param>
    /// <param name="command">The cancel command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelGroupMarketCommand command, CancellationToken cancellationToken)
    {
        if (id != command.MarketId) return BadRequest();
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(command with { RequestingUserId = callerId }, cancellationToken);
        return NoContent();
    }
}
