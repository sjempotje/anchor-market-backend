namespace AnchorMarket.Domain.Entities;

public class Verification : BaseEntity
{
    public string Identifier { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }

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
