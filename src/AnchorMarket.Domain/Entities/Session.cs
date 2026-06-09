namespace AnchorMarket.Domain.Entities;

public class Session : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public User User { get; private set; } = null!;

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
