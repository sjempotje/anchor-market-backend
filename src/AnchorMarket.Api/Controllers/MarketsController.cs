using AnchorMarket.Application.Features.Markets.Commands;
using AnchorMarket.Application.Features.Markets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages prediction markets.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarketsController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves all markets.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of markets.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetMarketsQuery(), cancellationToken));

    /// <summary>Retrieves a market by its ID.</summary>
    /// <param name="id">The market ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The market if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var market = await sender.Send(new GetMarketByIdQuery(id), cancellationToken);
        return market is null ? NotFound() : Ok(market);
    }

    /// <summary>Creates a new market.</summary>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 response with the new market ID.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMarketCommand command, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var id = await sender.Send(command with { CreatorId = callerId }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>Updates an existing market.</summary>
    /// <param name="id">The market ID.</param>
    /// <param name="command">The update command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMarketCommand command, CancellationToken cancellationToken)
    {
        if (id != command.MarketId) return BadRequest();
        await sender.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a market by ID.</summary>
    /// <param name="id">The market ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteMarketCommand(id), cancellationToken);
        return NoContent();
    }
}
