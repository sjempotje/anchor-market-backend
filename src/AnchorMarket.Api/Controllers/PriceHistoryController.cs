using AnchorMarket.Application.Features.PriceHistory.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>Retrieves historical price data for market outcomes.</summary>
[ApiController]
[Route("api/outcomes/{outcomeId:guid}/price-history")]
public class PriceHistoryController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves price history for a specific outcome.</summary>
    /// <param name="outcomeId">The outcome ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of price history entries.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetByOutcome(Guid outcomeId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetPriceHistoryByOutcomeQuery(outcomeId), cancellationToken));
}
