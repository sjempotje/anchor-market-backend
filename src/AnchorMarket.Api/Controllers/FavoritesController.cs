using AnchorMarket.Application.Features.Favorites.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages favorited markets and teams for the authenticated user.</summary>
[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController(ISender sender) : ControllerBase
{
    /// <summary>Toggles a market as a favorite for the authenticated user.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether the market was favorited.</returns>
    [HttpPost("markets/{marketId:guid}")]
    public async Task<IActionResult> ToggleMarket(Guid marketId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var added = await sender.Send(new ToggleFavoriteMarketCommand(userId, marketId), cancellationToken);
        return Ok(new { favorited = added });
    }

    /// <summary>Toggles a team as a favorite for the authenticated user.</summary>
    /// <param name="teamId">The team ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether the team was favorited.</returns>
    [HttpPost("teams/{teamId:guid}")]
    public async Task<IActionResult> ToggleTeam(Guid teamId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var added = await sender.Send(new ToggleFavoriteTeamCommand(userId, teamId), cancellationToken);
        return Ok(new { favorited = added });
    }
}
