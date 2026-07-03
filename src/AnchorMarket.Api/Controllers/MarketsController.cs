using System;
using AnchorMarket.Application.Features.Markets.Commands;
using AnchorMarket.Application.Features.Markets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

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
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
        => Ok(await sender.Send(new GetMarketsQuery(activeOnly), cancellationToken));

    /// <summary>Retrieves a market by its ID.</summary>
    /// <param name="id">The market ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The market if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var market = await sender.Send(new GetMarketByIdQuery(id, GetCallerId()), cancellationToken);
        return market is null ? NotFound() : Ok(market);
    }

    /// <summary>Retrieves the outcomes of a market.</summary>
    /// <param name="id">The market ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The market's outcomes, ordered for display.</returns>
    [HttpGet("{id:guid}/outcomes")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOutcomes(Guid id, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetMarketOutcomesQuery(id, GetCallerId()), cancellationToken));

    /// <summary>Retrieves an outcome's historical implied-probability price series.</summary>
    /// <param name="outcomeId">The outcome ID.</param>
    /// <param name="limit">Maximum number of most-recent points to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The outcome's price history, oldest first.</returns>
    [HttpGet("outcomes/{outcomeId:guid}/price-history")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOutcomePriceHistory(Guid outcomeId, [FromQuery] int limit = 500, CancellationToken cancellationToken = default)
        => Ok(await sender.Send(new GetOutcomePriceHistoryQuery(outcomeId, limit, GetCallerId()), cancellationToken));

    /// <summary>Retrieves a market's most recent trades across all outcomes.</summary>
    /// <param name="id">The market ID.</param>
    /// <param name="limit">Maximum number of most-recent trades to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The market's trades, most recent first.</returns>
    [HttpGet("{id:guid}/trades")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTrades(Guid id, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        => Ok(await sender.Send(new GetMarketTradesQuery(id, limit, GetCallerId()), cancellationToken));

    /// <summary>Resolves the authenticated caller's ID from claims, if any (endpoints here allow anonymous access for public markets).</summary>
    private Guid? GetCallerId()
        => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

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
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(command with { CallerId = callerId }, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a market by ID.</summary>
    /// <param name="id">The market ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(new DeleteMarketCommand(id, callerId), cancellationToken);
        return NoContent();
    }
}
