using AnchorMarket.Application.Features.Teams.Commands;
using AnchorMarket.Application.Features.Teams.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages sports teams.</summary>
[ApiController]
[Route("api/[controller]")]
public class TeamsController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves all teams.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of teams.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetTeamsQuery(), cancellationToken));

    /// <summary>Retrieves a team by its ID.</summary>
    /// <param name="id">The team ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The team if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new team.</summary>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 response with the new team ID.</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
}
