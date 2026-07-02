using System;
using AnchorMarket.Application.Features.Positions.Commands;
using AnchorMarket.Application.Features.Positions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages user positions in prediction markets.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PositionsController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves all positions for the authenticated user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await sender.Send(new GetPositionsQuery(userId), cancellationToken));
    }

    /// <summary>Retrieves a position by its ID.</summary>
    /// <param name="id">The position ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The position if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var position = await sender.Send(new GetPositionByIdQuery(id, callerId), cancellationToken);
        return position is null ? NotFound() : Ok(position);
    }

    /// <summary>Gets all positions for the authenticated user with calculated PnL.</summary>
    [HttpGet("with-pnl")]
    public async Task<IActionResult> GetWithPnL(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await sender.Send(new GetPositionsWithPnLQuery(userId), cancellationToken));
    }

    /// <summary>Retrieves positions by market for the authenticated user.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of positions.</returns>
    [HttpGet("by-market/{marketId:guid}")]
    public async Task<IActionResult> GetByMarket(Guid marketId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await sender.Send(new GetPositionsByMarketQuery(marketId, userId), cancellationToken));
    }

    /// <summary>Places a new position in a market.</summary>
    /// <param name="command">The place position command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 response with the new position ID.</returns>
    [HttpPost]
    public async Task<IActionResult> Place([FromBody] PlacePositionCommand command, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var id = await sender.Send(command with { UserId = callerId }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>Closes an open position.</summary>
    /// <param name="id">The position ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(new ClosePositionCommand(id, userId), cancellationToken);
        return NoContent();
    }
}
