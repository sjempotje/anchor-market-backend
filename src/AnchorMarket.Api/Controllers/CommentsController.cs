using AnchorMarket.Application.Features.Comments.Commands;
using AnchorMarket.Application.Features.Comments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages comments on markets.</summary>
[ApiController]
[Route("api/markets/{marketId:guid}/comments")]
public class CommentsController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves all comments for a market.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of comments.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetByMarket(Guid marketId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetCommentsByMarketQuery(marketId), cancellationToken));

    /// <summary>Creates a new comment on a market.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 response with the new comment ID.</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Guid marketId, [FromBody] CreateCommentCommand command,
        CancellationToken cancellationToken)
    {
        if (marketId != command.MarketId) return BadRequest();
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (command.UserId != callerId) return Forbid();
        var id = await sender.Send(command, cancellationToken);
        return Ok(new { id });
    }

    /// <summary>Deletes a comment by ID.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="id">The comment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid marketId, Guid id, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(new DeleteCommentCommand(id, callerId), cancellationToken);
        return NoContent();
    }
}
