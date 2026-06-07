using AnchorMarket.Application.Features.Positions.Commands;
using AnchorMarket.Application.Features.Positions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnchorMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly ISender _sender;

    public PositionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
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
        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User not authenticated.");

        var positions = await _sender.Send(new GetPositionsWithPnLQuery(Guid.Parse(userId)), cancellationToken);
        return Ok(positions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var position = await _sender.Send(new GetPositionByIdQuery(id), cancellationToken);
        return position is null ? NotFound() : Ok(position);
    }

    [HttpGet("by-market/{marketId:guid}")]
    public async Task<IActionResult> GetByMarket(Guid marketId, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
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
    public async Task<IActionResult> Close(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ClosePositionCommand(id, userId), cancellationToken);
        return NoContent();
    }
}
