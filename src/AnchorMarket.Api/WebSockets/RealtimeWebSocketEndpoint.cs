using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnchorMarket.Api.WebSockets;

/// <summary>Handles the raw WebSocket endpoint: authentication, subscription routing, and lifecycle.</summary>
public static class RealtimeWebSocketEndpoint
{
    private static readonly JsonSerializerOptions ParseOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Accepts a WebSocket connection and processes its subscription messages until it closes.</summary>
    public static async Task HandleAsync(
        HttpContext context,
        WebSocketConnectionManager manager,
        IServiceScopeFactory scopeFactory,
        ILogger logger)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (context.User.Identity?.IsAuthenticated != true || !Guid.TryParse(userIdValue, out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var connection = new WebSocketConnection(socket, userId);
        manager.Add(connection);
        logger.LogDebug("WebSocket {ConnectionId} connected (user {UserId}).", connection.Id, userId);

        try
        {
            await ReceiveLoopAsync(connection, socket, manager, scopeFactory, logger, context.RequestAborted);
        }
        catch (OperationCanceledException) { /* client disconnected */ }
        catch (WebSocketException) { /* abrupt close */ }
        finally
        {
            manager.Remove(connection.Id);
            // Complete the close handshake whether we initiated it or are responding to the client.
            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                try
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                catch (WebSocketException) { /* already gone */ }
                catch (OperationCanceledException) { /* already gone */ }
            }
            logger.LogDebug("WebSocket {ConnectionId} disconnected.", connection.Id);
        }
    }

    private static async Task ReceiveLoopAsync(
        WebSocketConnection connection,
        WebSocket socket,
        WebSocketConnectionManager manager,
        IServiceScopeFactory scopeFactory,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            using var message = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                    return;
                message.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            await HandleMessageAsync(connection, Encoding.UTF8.GetString(message.ToArray()), scopeFactory, cancellationToken);
        }
    }

    private static async Task HandleMessageAsync(
        WebSocketConnection connection,
        string text,
        IServiceScopeFactory scopeFactory,
        CancellationToken cancellationToken)
    {
        SubscriptionRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<SubscriptionRequest>(text, ParseOptions);
        }
        catch (JsonException)
        {
            await SendAsync(connection, new { type = "error", message = "Malformed message." }, cancellationToken);
            return;
        }

        if (request?.Action is not { } action)
        {
            await SendAsync(connection, new { type = "error", message = "Missing 'action'." }, cancellationToken);
            return;
        }

        var topic = RealtimeTopics.Resolve(request);
        if (topic is null)
        {
            await SendAsync(connection, new { type = "error", message = "Invalid channel or missing identifier." }, cancellationToken);
            return;
        }

        switch (action.ToLowerInvariant())
        {
            case "subscribe":
                if (!await IsAuthorizedAsync(scopeFactory, request, connection.UserId, cancellationToken))
                {
                    await SendAsync(connection, new { type = "error", message = "Not authorized to subscribe to this group market." }, cancellationToken);
                    return;
                }
                connection.Topics[topic] = 0;
                await SendAsync(connection, new { type = "subscribed", topic }, cancellationToken);
                break;

            case "unsubscribe":
                connection.Topics.TryRemove(topic, out _);
                await SendAsync(connection, new { type = "unsubscribed", topic }, cancellationToken);
                break;

            default:
                await SendAsync(connection, new { type = "error", message = $"Unknown action '{action}'." }, cancellationToken);
                break;
        }
    }

    /// <summary>
    /// Authorizes a subscription. Topics over a group-scoped market (resolved via the requested
    /// market/outcome/group) are only allowed for members of that group; public markets are open.
    /// </summary>
    private static async Task<bool> IsAuthorizedAsync(
        IServiceScopeFactory scopeFactory, SubscriptionRequest request, Guid userId, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Explicit group channel: membership of the named group.
        if (string.Equals(request.Channel, "group-market", StringComparison.OrdinalIgnoreCase))
            return request.GroupId is { } g && await IsMemberAsync(db, userId, g, cancellationToken);

        // Find the market behind the subscription (directly or via the outcome).
        var marketId = request.MarketId;
        if (marketId is null && request.OutcomeId is { } outcomeId)
        {
            marketId = await db.Outcomes
                .Where(o => o.Id == outcomeId)
                .Select(o => (Guid?)o.MarketId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (marketId is null)
            return true; // unknown target — there is nothing private to leak

        var market = await db.Markets
            .Where(m => m.Id == marketId)
            .Select(m => new { m.Scope, m.GroupId })
            .FirstOrDefaultAsync(cancellationToken);

        if (market is null || market.Scope != MarketScope.Group)
            return true; // public market (or not found) — open

        return market.GroupId is { } groupId && await IsMemberAsync(db, userId, groupId, cancellationToken);
    }

    private static Task<bool> IsMemberAsync(IApplicationDbContext db, Guid userId, Guid groupId, CancellationToken cancellationToken)
        => db.GroupMemberships.AnyAsync(m => m.UserId == userId && m.GroupId == groupId, cancellationToken);

    private static Task SendAsync(WebSocketConnection connection, object payload, CancellationToken cancellationToken)
        => connection.SendAsync(JsonSerializer.Serialize(payload), cancellationToken);
}
