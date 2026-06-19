namespace AnchorMarket.Domain.Entities;

/// <summary>An authenticated user session identified by a token with an expiration time.</summary>
public class Session : BaseEntity
{
    /// <summary>Gets the ID of the associated user.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the session token used for authentication.</summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>Gets the date and time when this session expires.</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>Gets the IP address from which the session was created.</summary>
    public string? IpAddress { get; private set; }

    /// <summary>Gets the user agent string from which the session was created.</summary>
    public string? UserAgent { get; private set; }

    /// <summary>Gets the associated user.</summary>
    public User User { get; private set; } = null!;

    /// <summary>Creates a new session.</summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="token">The session token.</param>
    /// <param name="expiresAt">The session expiration time.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <param name="userAgent">The user agent of the client.</param>
    /// <returns>A new <see cref="Session"/> instance.</returns>
    public static Session Create(Guid userId, string token, DateTimeOffset expiresAt, string? ipAddress, string? userAgent)
    {
        return new Session
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
    }
}
