using AnchorMarket.Application.Features.Markets.Commands;
using AnchorMarket.Application.Features.Markets.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketsController : ControllerBase
{
    private readonly ISender _sender;

    public MarketsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var markets = await _sender.Send(new GetMarketsQuery(), cancellationToken);
        return Ok(markets);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var market = await _sender.Send(new GetMarketByIdQuery(id), cancellationToken);
        return market is null ? NotFound() : Ok(market);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMarketCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMarketCommand command, CancellationToken cancellationToken)
    {
        if (id != command.MarketId) return BadRequest();
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteMarketCommand(id), cancellationToken);
        return NoContent();
    }
}
