using AnchorMarket.Application.Features.Positions.Commands;
using AnchorMarket.Application.Features.Positions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnchorMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PositionsController : ControllerBase
{
    private readonly ISender _sender;

    public PositionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var positions = await _sender.Send(new GetPositionsQuery(), cancellationToken);
        return Ok(positions);
    }

    /// <summary>
    /// Gets all positions for the current user with calculated PnL information.
    /// </summary>
    [HttpGet("with-pnl")]
    public async Task<IActionResult> GetPositionsWithPnL(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var positions = await _sender.Send(new GetPositionsWithPnLQuery(userId), cancellationToken);
        return Ok(positions);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var position = await _sender.Send(new GetPositionByIdQuery(id), cancellationToken);
        return position is null ? NotFound() : Ok(position);
    }

    [HttpGet("by-market/{marketId:guid}")]
    public async Task<IActionResult> GetByMarket(Guid marketId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var positions = await _sender.Send(new GetPositionsByMarketQuery(marketId, userId), cancellationToken);
        return Ok(positions);
    }

    [HttpPost]
    public async Task<IActionResult> Place([FromBody] PlacePositionCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _sender.Send(new ClosePositionCommand(id, userId), cancellationToken);
        return NoContent();
    }
}
