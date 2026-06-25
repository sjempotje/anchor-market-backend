using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace AnchorMarket.Api.WebSockets;

/// <summary>Represents a single connected WebSocket client and the topics it is subscribed to.</summary>
public sealed class WebSocketConnection(WebSocket socket, Guid userId)
{
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    /// <summary>Gets the unique identifier for this connection.</summary>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>Gets the authenticated user behind this connection.</summary>
    public Guid UserId { get; } = userId;

    /// <summary>Gets the underlying socket.</summary>
    public WebSocket Socket => socket;

    /// <summary>Gets the set of topics this connection is subscribed to.</summary>
    public ConcurrentDictionary<string, byte> Topics { get; } = new();

    /// <summary>Sends a text message to the client, serializing concurrent sends on this socket.</summary>
    /// <param name="message">The UTF-8 text payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        if (socket.State != WebSocketState.Open)
            return;

        var bytes = Encoding.UTF8.GetBytes(message);
        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
        }
        finally
        {
            _sendLock.Release();
        }
    }
}
