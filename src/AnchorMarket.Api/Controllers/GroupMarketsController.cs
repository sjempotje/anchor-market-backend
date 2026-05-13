using AnchorMarket.Application.Features.GroupMarkets.Commands;
using AnchorMarket.Application.Features.GroupMarkets.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

[ApiController]
[Route("api/group-markets")]
public class GroupMarketsController : ControllerBase
{
    private readonly ISender _sender;

    public GroupMarketsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var market = await _sender.Send(new GetGroupMarketByIdQuery(id), cancellationToken);
        return market is null ? NotFound() : Ok(market);
    }

    [HttpGet]
    public async Task<IActionResult> GetByGroup(
        [FromQuery] Guid groupId,
        [FromQuery] Guid requestingUserId,
        CancellationToken cancellationToken)
    {
        var markets = await _sender.Send(new GetGroupMarketsQuery(groupId, requestingUserId), cancellationToken);
        return Ok(markets);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupMarketCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>
    /// Resolves a group market. The ResolverId must be a group member who is NOT the market creator.
    /// Two-person rule is enforced in the Application layer, not here.
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveGroupMarketCommand command, CancellationToken cancellationToken)
    {
        if (id != command.MarketId) return BadRequest();
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelGroupMarketCommand command, CancellationToken cancellationToken)
    {
        if (id != command.MarketId) return BadRequest();
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }
}
