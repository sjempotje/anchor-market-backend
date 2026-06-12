using AnchorMarket.Application.Features.Orders.Commands;
using MediatR;
using AnchorMarket.Application.Features.Orders.DTOs;
using AnchorMarket.Application.Features.Orders.Queries;
using AnchorMarket.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnchorMarket.Api.Controllers;

/// <summary>
/// API for managing limit orders on prediction market outcomes.
/// Supports placing, canceling, and querying limit orders.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LimitOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public LimitOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Places a new limit order to buy or sell shares on a specific outcome.
    /// Buy orders debit the user's wallet; sell orders require sufficient position holdings.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceLimitOrderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new PlaceLimitOrderCommand(
            userId,
            request.MarketId,
            request.OutcomeId,
            request.Side,
            request.Price,
            request.Quantity,
            request.ExpiresAt);

        var orderId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { orderId }, orderId);
    }

    /// <summary>
    /// Cancels an existing limit order, releasing reserved funds.
    /// </summary>
    [HttpDelete("{orderId}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CancelLimitOrderCommand(orderId, userId);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Retrieves a specific limit order by ID.
    /// </summary>
    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(LimitOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetLimitOrderDetailQuery(orderId, userId);
        var order = await _mediator.Send(query, cancellationToken);

        return Ok(order);
    }

    /// <summary>
    /// Retrieves limit orders for a specific market, optionally filtered by outcome.
    /// </summary>
    [HttpGet("market/{marketId}")]
    [ProducesResponseType(typeof(IReadOnlyList<LimitOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrdersByMarket(
        Guid marketId,
        [FromQuery] Guid? outcomeId = null,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetLimitOrdersByMarketQuery(marketId, outcomeId, userId);
        var orders = await _mediator.Send(query, cancellationToken);

        return Ok(orders);
    }
}

/// <summary>
/// Request model for placing a limit order.
/// </summary>
public record PlaceLimitOrderRequest(
    Guid MarketId,
    Guid OutcomeId,
    OrderSide Side,
    decimal Price,
    decimal Quantity,
    DateTimeOffset? ExpiresAt);
