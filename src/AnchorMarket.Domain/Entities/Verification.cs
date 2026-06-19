namespace AnchorMarket.Domain.Entities;

/// <summary>A verification record used to confirm an identifier (e.g. email, phone) with a code before expiration.</summary>
public class Verification : BaseEntity
{
    /// <summary>Gets the identifier being verified (e.g. email address or phone number).</summary>
    public string Identifier { get; private set; } = string.Empty;

    /// <summary>Gets the verification code or token value.</summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>Gets the date and time when this verification expires.</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>Creates a new verification record.</summary>
    /// <param name="identifier">The identifier to verify.</param>
    /// <param name="value">The verification code or token.</param>
    /// <param name="expiresAt">The expiration time.</param>
    /// <returns>A new <see cref="Verification"/> instance.</returns>
    public static Verification Create(string identifier, string value, DateTimeOffset expiresAt)
    {
        return new Verification
        {
            Identifier = identifier,
            Value = value,
            ExpiresAt = expiresAt
        };
    }
}
