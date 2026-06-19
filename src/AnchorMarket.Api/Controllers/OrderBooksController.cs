using AnchorMarket.Application.Features.Orders.Commands;
using AnchorMarket.Application.Features.Orders.DTOs;
using AnchorMarket.Application.Features.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>Queries order book data and market prices.</summary>
[ApiController]
[Route("api/[controller]")]
public class OrderBooksController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves the order book for a specific market outcome.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="outcomeId">The outcome ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order book with bid/ask levels.</returns>
    [HttpGet("market/{marketId:guid}/outcome/{outcomeId:guid}")]
    [ProducesResponseType(typeof(OrderBookDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderBook(
        Guid marketId, Guid outcomeId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetOrderBookQuery(marketId, outcomeId), cancellationToken));

    /// <summary>Retrieves the current market price for a specific outcome.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="outcomeId">The outcome ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The market price.</returns>
    [HttpGet("market/{marketId:guid}/outcome/{outcomeId:guid}/price")]
    [ProducesResponseType(typeof(MarketPriceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMarketPrice(
        Guid marketId, Guid outcomeId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetMarketPriceQuery(marketId, outcomeId), cancellationToken));

    /// <summary>Triggers the order matching engine for a specific market.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="outcomeId">Optional outcome ID to match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching result.</returns>
    [HttpPost("market/{marketId:guid}/match")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MatchingResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> MatchOrders(
        Guid marketId,
        [FromQuery] Guid? outcomeId = null,
        CancellationToken cancellationToken = default)
        => Ok(await sender.Send(new MatchOrdersCommand(marketId, outcomeId), cancellationToken));
}
