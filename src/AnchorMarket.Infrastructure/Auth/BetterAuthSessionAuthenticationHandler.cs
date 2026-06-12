using System.Security.Claims;
using System.Text.Encodings.Web;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnchorMarket.Infrastructure.Auth;

/// <summary>
/// Authenticates requests using Better Auth session tokens.
/// Accepts the token via <c>Authorization: Bearer &lt;token&gt;</c> header
/// or the <c>better-auth.session_token</c> cookie as a fallback.
/// If the authenticated user has no wallet, one is created automatically.
/// </summary>
public class BetterAuthSessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ApplicationDbContext db)
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
        else if (Request.Cookies.TryGetValue("better-auth.session_token", out var cookieToken))
        {
            token = cookieToken;
        }

        if (string.IsNullOrWhiteSpace(token))
            return AuthenticateResult.NoResult();

        var session = await db.Sessions
            .Include(s => s.User)
                .ThenInclude(u => u.Wallet)
            .FirstOrDefaultAsync(s => s.Token == token);

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
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
