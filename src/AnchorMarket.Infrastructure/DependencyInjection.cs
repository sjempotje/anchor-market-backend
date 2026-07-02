using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Infrastructure.Auth;
using AnchorMarket.Infrastructure.BackgroundServices;
using AnchorMarket.Infrastructure.Persistence;
using AnchorMarket.Infrastructure.Realtime;
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

        AddRealtime(services, configuration);
        AddBackgroundServices(services, configuration);

        return services;
    }

    /// <summary>
    /// Registers long-running hosted services. Disabled when "BackgroundServices:Enabled" is "false"
    /// (e.g. integration tests, where background loops would race the shared test database).
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The application configuration.</param>
    private static void AddBackgroundServices(IServiceCollection services, IConfiguration configuration)
    {
        var enabled = !string.Equals(configuration["BackgroundServices:Enabled"], "false", StringComparison.OrdinalIgnoreCase);
        // Skip when no database is configured (e.g. build-time OpenAPI generation, design-time tooling),
        // where booting the host would spam connection failures.
        var hasDatabase = !string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection"));
        if (!enabled || !hasDatabase)
            return;

        services.AddHostedService<VolumeStatsUpdaterService>();
        services.AddHostedService<PartitionManagerService>();
    }

    /// <summary>
    /// Registers the real-time cache and publisher using no-op implementations.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The application configuration.</param>
    private static void AddRealtime(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IOrderBookCache, NullOrderBookCache>();
        services.AddSingleton<IRealtimePublisher, NullRealtimePublisher>();
    }
}
