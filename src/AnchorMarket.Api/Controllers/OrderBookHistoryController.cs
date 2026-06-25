using AnchorMarket.Application.Features.OrderBookHistory.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>Serves historical order book snapshots for charting.</summary>
[ApiController]
[Route("api/orderbook/history")]
[Authorize]
public class OrderBookHistoryController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves recent order book snapshots for an outcome, oldest first.</summary>
    /// <param name="outcomeId">The outcome ID.</param>
    /// <param name="limit">Maximum number of snapshots to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The outcome's order book snapshots.</returns>
    [HttpGet("{outcomeId:guid}")]
    public async Task<IActionResult> GetByOutcome(Guid outcomeId, [FromQuery] int limit = 200, CancellationToken cancellationToken = default)
        => Ok(await sender.Send(new GetOrderBookSnapshotsQuery(outcomeId, limit), cancellationToken));
}
