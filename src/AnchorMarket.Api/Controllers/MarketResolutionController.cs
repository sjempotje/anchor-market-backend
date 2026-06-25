using System.Security.Claims;
using AnchorMarket.Application.Features.MarketResolutions.Commands;
using AnchorMarket.Application.Features.MarketResolutions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>Resolves public markets and exposes their resolution result.</summary>
[ApiController]
[Route("api/markets")]
public class MarketResolutionController(ISender sender) : ControllerBase
{
    /// <summary>Resolves a public market with the winning outcome (admin only).</summary>
    /// <param name="marketId">The market to resolve.</param>
    /// <param name="request">The resolution request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{marketId:guid}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Resolve(Guid marketId, [FromBody] ResolveMarketRequest request, CancellationToken cancellationToken)
    {
        var resolvedById = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(
            new ResolvePublicMarketCommand(marketId, request.WinningOutcomeId, resolvedById, request.ResolutionSource, request.ResolutionNotes),
            cancellationToken);
        return NoContent();
    }

    /// <summary>Retrieves how a market was resolved, including the winning outcome.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolution if the market is resolved; otherwise 404.</returns>
    [HttpGet("{marketId:guid}/resolution")]
    [AllowAnonymous]
    public async Task<IActionResult> GetResolution(Guid marketId, CancellationToken cancellationToken)
    {
        var resolution = await sender.Send(new GetMarketResolutionQuery(marketId), cancellationToken);
        return resolution is null ? NotFound() : Ok(resolution);
    }
}

/// <summary>Request body for resolving a public market.</summary>
public record ResolveMarketRequest(Guid WinningOutcomeId, string? ResolutionSource = null, string? ResolutionNotes = null);
