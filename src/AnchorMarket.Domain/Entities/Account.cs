namespace AnchorMarket.Domain.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; private set; }
    public string AccountId { get; private set; } = string.Empty;
    public string ProviderId { get; private set; } = string.Empty;
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTimeOffset? AccessTokenExpiresAt { get; private set; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; private set; }
    public string? Scope { get; private set; }
    public string? IdToken { get; private set; }
    public string? Password { get; private set; }

    public User User { get; private set; } = null!;

    public static Account Create(Guid userId, string accountId, string providerId)
    {
        return new Account
        {
            UserId = userId,
            AccountId = accountId,
            ProviderId = providerId
        };
    }
}
