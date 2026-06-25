using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                catch (WebSocketException) { /* already gone */ }
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

        var topic = RealtimeTopics.Resolve(request, out var requiresMembership, out var groupId);
        if (topic is null)
        {
            await SendAsync(connection, new { type = "error", message = "Invalid channel or missing identifier." }, cancellationToken);
            return;
        }

        switch (action.ToLowerInvariant())
        {
            case "subscribe":
                if (requiresMembership && !await IsGroupMemberAsync(scopeFactory, connection.UserId, groupId, cancellationToken))
                {
                    await SendAsync(connection, new { type = "error", message = "Not a member of this group." }, cancellationToken);
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

    private static async Task<bool> IsGroupMemberAsync(IServiceScopeFactory scopeFactory, Guid userId, Guid groupId, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        return await db.GroupMemberships.AnyAsync(m => m.UserId == userId && m.GroupId == groupId, cancellationToken);
    }

    private static Task SendAsync(WebSocketConnection connection, object payload, CancellationToken cancellationToken)
        => connection.SendAsync(JsonSerializer.Serialize(payload), cancellationToken);
}
