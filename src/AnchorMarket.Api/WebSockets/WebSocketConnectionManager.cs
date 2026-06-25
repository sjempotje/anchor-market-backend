using System.Collections.Concurrent;

namespace AnchorMarket.Api.WebSockets;

/// <summary>
/// Tracks active WebSocket connections and fans broadcasts out to the connections subscribed to a
/// given topic. Registered as a singleton and shared between the WebSocket endpoint and the
/// real-time backplane.
/// </summary>
public sealed class WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
{
    private readonly ConcurrentDictionary<string, WebSocketConnection> _connections = new();

    /// <summary>Registers a connection.</summary>
    public void Add(WebSocketConnection connection) => _connections[connection.Id] = connection;

    /// <summary>Removes a connection.</summary>
    public void Remove(string connectionId) => _connections.TryRemove(connectionId, out _);

    /// <summary>Gets the number of active connections.</summary>
    public int Count => _connections.Count;

    /// <summary>Sends a message to every connection subscribed to the given topic.</summary>
    /// <param name="topic">The topic to broadcast to.</param>
    /// <param name="message">The serialized message payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task BroadcastAsync(string topic, string message, CancellationToken cancellationToken = default)
    {
        foreach (var connection in _connections.Values)
        {
            if (!connection.Topics.ContainsKey(topic))
                continue;

            try
            {
                await connection.SendAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Dropping connection {ConnectionId} after a failed send.", connection.Id);
                Remove(connection.Id);
            }
        }
    }
}
