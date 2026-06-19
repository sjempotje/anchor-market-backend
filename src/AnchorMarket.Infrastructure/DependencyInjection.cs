using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Infrastructure.Auth;
using AnchorMarket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnchorMarket.Infrastructure;

/// <summary>Registers infrastructure-layer services including EF Core, authentication, and authorization.</summary>
public static class DependencyInjection
{
    /// <summary>The custom authentication scheme name used by the application.</summary>
    public const string SchemeName = "BetterAuth";

    /// <summary>Configures and registers infrastructure services for the application.</summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");

            if (connectionString is null)
                options.UseNpgsql();
            else if (connectionString.StartsWith("DataSource="))
                options.UseSqlite(connectionString);
            else
                options.UseNpgsql(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddAuthentication(SchemeName)
            .AddScheme<AuthenticationSchemeOptions, BetterAuthSessionAuthenticationHandler>(SchemeName, _ => { });

        services.AddAuthorization();

        return services;
    }
}
