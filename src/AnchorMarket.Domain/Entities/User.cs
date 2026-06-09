namespace AnchorMarket.Domain.Entities;

/// <summary>Represents an application user with authentication and wallet capabilities.</summary>
public class User : BaseEntity
{
    private static readonly int MaxUsernameLength = 100;
    private static readonly int MaxEmailLength = 255;

    public string? Username { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool EmailVerified { get; private set; }
    public string? Image { get; private set; }

    /// <summary>
    /// Navigation property to the user's wallet.
    /// </summary>
    public Wallet? Wallet { get; private set; }

    public ICollection<Session> Sessions { get; private set; } = new List<Session>();
    public ICollection<Account> Accounts { get; private set; } = new List<Account>();

    /// <summary>
    /// Static factory method to create a new user with validation.
    /// </summary>
    public static User Create(string username, string email)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty or whitespace.", nameof(username));

        if (username.Length > MaxUsernameLength)
            throw new ArgumentException($"Username cannot exceed {MaxUsernameLength} characters.", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty or whitespace.", nameof(email));

        if (email.Length > MaxEmailLength)
            throw new ArgumentException($"Email cannot exceed {MaxEmailLength} characters.", nameof(email));

        return new User
        {
            Username = username.Trim(),
            Name = username.Trim(),
            Email = email.Trim().ToLowerInvariant()
        };
    }

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    public void Update(string username, string email)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty or whitespace.", nameof(username));

        if (username.Length > MaxUsernameLength)
            throw new ArgumentException($"Username cannot exceed {MaxUsernameLength} characters.", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty or whitespace.", nameof(email));

        if (email.Length > MaxEmailLength)
            throw new ArgumentException($"Email cannot exceed {MaxEmailLength} characters.", nameof(email));

        Username = username.Trim();
        Name = username.Trim();
        Email = email.Trim().ToLowerInvariant();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Creates a new wallet for this user.
    /// </summary>
    public Wallet CreateWallet()
    {
        var wallet = Wallet.Create(Id);
        Wallet = wallet;
        return wallet;
    }
}
