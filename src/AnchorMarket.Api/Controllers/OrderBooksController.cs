using AnchorMarket.Application.Features.Orders.Commands;
using MediatR;
using AnchorMarket.Application.Features.Orders.DTOs;
using AnchorMarket.Application.Features.Orders.Queries;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

/// <summary>
/// API for querying order book data and market prices.
/// Provides real-time order book depth, best bid/ask, and price statistics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderBooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderBooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves the full order book for a specific market outcome.
    /// Returns aggregated bid/ask levels sorted by price.
    /// </summary>
    [HttpGet("market/{marketId}/outcome/{outcomeId}")]
    [ProducesResponseType(typeof(OrderBookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderBook(
        Guid marketId,
        Guid outcomeId,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderBookQuery(marketId, outcomeId);
        var orderBook = await _mediator.Send(query, cancellationToken);

        return Ok(orderBook);
    }

    /// <summary>
    /// Retrieves the current market price for a specific outcome.
    /// Uses average entry price from positions as the price indicator.
    /// </summary>
    [HttpGet("market/{marketId}/outcome/{outcomeId}/price")]
    [ProducesResponseType(typeof(MarketPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMarketPrice(
        Guid marketId,
        Guid outcomeId,
        CancellationToken cancellationToken)
    {
        var query = new GetMarketPriceQuery(marketId, outcomeId);
        var price = await _mediator.Send(query, cancellationToken);

        return Ok(price);
    }

    /// <summary>
    /// Triggers the order matching engine for a specific market.
    /// This endpoint is intended for internal/testing use; consider securing it in production.
    /// </summary>
    [HttpPost("market/{marketId}/match")]
    [ProducesResponseType(typeof(MatchingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MatchOrders(
        Guid marketId,
        [FromQuery] Guid? outcomeId = null,
        CancellationToken cancellationToken = default)
    {
        var command = new MatchOrdersCommand(marketId, outcomeId);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
