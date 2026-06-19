using AnchorMarket.Application.Features.Events.Commands;
using AnchorMarket.Application.Features.Events.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages prediction events.</summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves all events.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of events.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetEventsQuery(), cancellationToken));

    /// <summary>Retrieves an event by its ID.</summary>
    /// <param name="id">The event ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The event if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEventByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new event.</summary>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 response with the new event ID.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateEventCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
}
