using AnchorMarket.Application.Features.Sports.Commands;
using AnchorMarket.Application.Features.Sports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages sports.</summary>
[ApiController]
[Route("api/[controller]")]
public class SportsController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves all sports.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of sports.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetSportsQuery(), cancellationToken));

    /// <summary>Retrieves a sport by its ID.</summary>
    /// <param name="id">The sport ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sport if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSportByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new sport.</summary>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 response with the new sport ID.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSportCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
}
