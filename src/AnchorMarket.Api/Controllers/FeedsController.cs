using AnchorMarket.Application.Features.ExternalFeeds.Commands;
using AnchorMarket.Application.Features.ExternalFeeds.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages external data feed registrations for markets.</summary>
[ApiController]
[Route("api/[controller]")]
public class FeedsController(ISender sender) : ControllerBase
{
    /// <summary>Registers an external feed for a market.</summary>
    /// <param name="command">The registration command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 response with the new feed registration ID.</returns>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterFeedCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>Retrieves a feed registration by its ID.</summary>
    /// <param name="id">The feed registration ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The feed registration if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFeedByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Retrieves all feed registrations for a market.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The market's feed registrations.</returns>
    [HttpGet("market/{marketId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetByMarket(Guid marketId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetFeedsByMarketQuery(marketId), cancellationToken));

    /// <summary>Retrieves recent raw results captured from a feed, newest first.</summary>
    /// <param name="id">The feed registration ID.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The recent feed results.</returns>
    [HttpGet("{id:guid}/results")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetResults(Guid id, [FromQuery] int limit = 100, CancellationToken cancellationToken = default)
        => Ok(await sender.Send(new GetFeedResultsQuery(id, limit), cancellationToken));

    /// <summary>Performs a dry-run fetch against a feed to validate its configuration.</summary>
    /// <param name="id">The feed registration ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The fetch result, including any parse error.</returns>
    [HttpPost("{id:guid}/test")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Test(Guid id, CancellationToken cancellationToken)
        => Ok(await sender.Send(new TestFeedCommand(id), cancellationToken));

    /// <summary>Updates an existing feed registration.</summary>
    /// <param name="id">The feed registration ID.</param>
    /// <param name="command">The update command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFeedCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route ID does not match command ID.");
        await sender.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a feed registration by ID.</summary>
    /// <param name="id">The feed registration ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteFeedCommand(id), cancellationToken);
        return NoContent();
    }
}
