using AnchorMarket.Application.Features.Users.Commands;
using AnchorMarket.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages user profiles.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves a user profile by ID.</summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user profile if found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await sender.Send(new GetUserProfileQuery(id), cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Updates a user profile.</summary>
    /// <param name="id">The user ID.</param>
    /// <param name="command">The update command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id != command.UserId || id != callerId) return Forbid();
        await sender.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a user account.</summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id != callerId) return Forbid();
        await sender.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }
}
