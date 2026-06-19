using AnchorMarket.Application.Features.Matches.Commands;
using AnchorMarket.Application.Features.Matches.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages sports matches.</summary>
[ApiController]
[Route("api/[controller]")]
public class MatchesController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves all matches.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matches.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetMatchesQuery(), cancellationToken));

    /// <summary>Retrieves a match by its ID.</summary>
    /// <param name="id">The match ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The match if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMatchByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new match.</summary>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 response with the new match ID.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateMatchCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>Updates the state of a match.</summary>
    /// <param name="id">The match ID.</param>
    /// <param name="command">The update state command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpPut("{id:guid}/state")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateState(Guid id, [FromBody] UpdateMatchStateCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.MatchId) return BadRequest();
        await sender.Send(command, cancellationToken);
        return NoContent();
    }
}
