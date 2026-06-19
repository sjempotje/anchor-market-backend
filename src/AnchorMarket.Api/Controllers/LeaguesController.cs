using AnchorMarket.Application.Features.Leagues.Commands;
using AnchorMarket.Application.Features.Leagues.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages sports leagues.</summary>
[ApiController]
[Route("api/[controller]")]
public class LeaguesController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves all leagues.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of leagues.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetLeaguesQuery(), cancellationToken));

    /// <summary>Retrieves a league by its ID.</summary>
    /// <param name="id">The league ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The league if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLeagueByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new league.</summary>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 response with the new league ID.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateLeagueCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
}
