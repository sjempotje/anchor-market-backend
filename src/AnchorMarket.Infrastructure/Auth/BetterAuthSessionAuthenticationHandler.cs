using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnchorMarket.Infrastructure.Auth;

/// <summary>
/// Authenticates requests using Better Auth session tokens.
/// Accepts the token via <c>Authorization: Bearer &lt;token&gt;</c> header,
/// or as a fallback the <c>__Secure-better-auth.session_token</c> cookie (HTTPS)
/// or <c>better-auth.session_token</c> cookie (HTTP/dev).
/// If the authenticated user has no wallet, one is created automatically.
/// </summary>
public class BetterAuthSessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ApplicationDbContext db,
    IConfiguration configuration)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? token = null;

        var authHeader = Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authHeader["Bearer ".Length..].Trim();
        }
        else if (Request.Cookies.TryGetValue("__Secure-better-auth.session_token", out var secureToken))
        {
            token = secureToken;
        }
        else if (Request.Cookies.TryGetValue("better-auth.session_token", out var cookieToken))
        {
            token = cookieToken;
        }
        else if (Request.Query.TryGetValue("token", out var queryToken))
        {
            token = queryToken;
        }

        if (string.IsNullOrWhiteSpace(token))
            return AuthenticateResult.NoResult();

        var decoded = Uri.UnescapeDataString(token);
        var parts = decoded.Split('.', 2);
        if (parts.Length != 2)
            return AuthenticateResult.Fail("Malformed session token.");

        var tokenId = parts[0];
        var signature = parts[1];

        var secret = configuration["Authentication:BetterAuthSecret"];
        if (string.IsNullOrEmpty(secret))
            return AuthenticateResult.Fail("Better Auth secret is not configured.");

        var expectedSignature = Convert.ToBase64String(
            HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(tokenId)));

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signature), Encoding.UTF8.GetBytes(expectedSignature)))
            return AuthenticateResult.Fail("Invalid session token signature.");

        var session = await db.Sessions
            .Include(s => s.User)
                .ThenInclude(u => u.Wallet)
            .FirstOrDefaultAsync(s => s.Token == tokenId);

        if (session is null)
            return AuthenticateResult.Fail("Invalid session token.");

        if (session.ExpiresAt < DateTimeOffset.UtcNow)
            return AuthenticateResult.Fail("Session has expired.");

        if (session.User.Wallet is null)
        {
            var wallet = Wallet.Create(session.UserId);
            db.Wallets.Add(wallet);
            await db.SaveChangesAsync();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new Claim(ClaimTypes.Name, session.User.Name),
            new Claim(ClaimTypes.Email, session.User.Email),
            new Claim(ClaimTypes.Role, session.User.Role.ToString()),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
