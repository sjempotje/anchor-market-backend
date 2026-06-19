namespace AnchorMarket.Domain.Entities;

/// <summary>An OAuth or credential-based authentication account linked to a user.</summary>
public class Account : BaseEntity
{
    /// <summary>Gets the ID of the user this account belongs to.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the provider-issued account identifier.</summary>
    public string AccountId { get; private set; } = string.Empty;

    /// <summary>Gets the authentication provider identifier (e.g. "google", "github").</summary>
    public string ProviderId { get; private set; } = string.Empty;

    /// <summary>Gets the OAuth access token, if available.</summary>
    public string? AccessToken { get; private set; }

    /// <summary>Gets the OAuth refresh token, if available.</summary>
    public string? RefreshToken { get; private set; }

    /// <summary>Gets the expiry time of the access token.</summary>
    public DateTimeOffset? AccessTokenExpiresAt { get; private set; }

    /// <summary>Gets the expiry time of the refresh token.</summary>
    public DateTimeOffset? RefreshTokenExpiresAt { get; private set; }

    /// <summary>Gets the OAuth scope granted for this account.</summary>
    public string? Scope { get; private set; }

    /// <summary>Gets the OIDC ID token, if available.</summary>
    public string? IdToken { get; private set; }

    /// <summary>Gets the hashed password for credential-based accounts.</summary>
    public string? Password { get; private set; }

    /// <summary>Gets the user this account belongs to.</summary>
    public User User { get; private set; } = null!;

    /// <summary>Creates a new account linked to the specified user and provider.</summary>
    /// <param name="userId">The ID of the owning user.</param>
    /// <param name="accountId">The provider-issued account identifier.</param>
    /// <param name="providerId">The authentication provider identifier.</param>
    /// <returns>A new <see cref="Account"/> instance.</returns>
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
