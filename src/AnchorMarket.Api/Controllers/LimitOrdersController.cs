using AnchorMarket.Application.Features.Orders.Commands;
using AnchorMarket.Application.Features.Orders.DTOs;
using AnchorMarket.Application.Features.Orders.Queries;
using AnchorMarket.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages limit orders on prediction market outcomes.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LimitOrdersController(ISender sender) : ControllerBase
{
    /// <summary>Places a new limit order to buy or sell shares on an outcome.</summary>
    /// <param name="request">The order details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 response with the new order ID.</returns>
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

        var orderId = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { orderId }, orderId);
    }

    /// <summary>Cancels an existing limit order, releasing reserved funds.</summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await sender.Send(new CancelLimitOrderCommand(orderId, userId), cancellationToken);
        return NoContent();
    }

    /// <summary>Retrieves a specific limit order by ID.</summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order details.</returns>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(LimitOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await sender.Send(new GetLimitOrderDetailQuery(orderId, userId), cancellationToken);
        return Ok(order);
    }

    /// <summary>Retrieves limit orders for a market, optionally filtered by outcome.</summary>
    /// <param name="marketId">The market ID.</param>
    /// <param name="outcomeId">Optional outcome ID to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of limit orders.</returns>
    [HttpGet("market/{marketId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<LimitOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByMarket(
        Guid marketId,
        [FromQuery] Guid? outcomeId = null,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var orders = await sender.Send(new GetLimitOrdersByMarketQuery(marketId, outcomeId, userId), cancellationToken);
        return Ok(orders);
    }
}

/// <summary>Request model for placing a limit order.</summary>
public record PlaceLimitOrderRequest(
    Guid MarketId,
    Guid OutcomeId,
    OrderSide Side,
    decimal Price,
    decimal Quantity,
    DateTimeOffset? ExpiresAt);
