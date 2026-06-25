using AnchorMarket.Application.Features.TradeFlow.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>Serves trade flow history (trades enriched with order book depth) for charting.</summary>
[ApiController]
[Route("api/trades/flow")]
[Authorize]
public class TradeFlowController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves recent trade flow snapshots for a market, oldest first.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="limit">Maximum number of snapshots to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The market's trade flow snapshots.</returns>
    [HttpGet("{marketId:guid}")]
    public async Task<IActionResult> GetByMarket(Guid marketId, [FromQuery] int limit = 200, CancellationToken cancellationToken = default)
        => Ok(await sender.Send(new GetTradeFlowsQuery(marketId, limit), cancellationToken));
}
