using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Infrastructure.Adapters;
using AnchorMarket.Infrastructure.Auth;
using AnchorMarket.Infrastructure.BackgroundServices;
using AnchorMarket.Infrastructure.Persistence;
using AnchorMarket.Infrastructure.Realtime;
using AnchorMarket.Infrastructure.Redis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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

        AddExternalFeeds(services);
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

        services.AddHostedService<FeedPollingService>();
        services.AddHostedService<ExpiredOrderCleanupService>();
        services.AddHostedService<OrderBookSnapshotService>();
        services.AddHostedService<PriceSnapshotService>();
        services.AddHostedService<VolumeStatsUpdaterService>();
    }

    /// <summary>
    /// Registers the real-time cache and publisher. When a "Redis" connection string is configured
    /// the Redis-backed implementations are used; otherwise no-op fallbacks keep the app (and tests)
    /// fully functional without a Redis server.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The application configuration.</param>
    private static void AddRealtime(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddSingleton<IOrderBookCache, NullOrderBookCache>();
            services.AddSingleton<IRealtimePublisher, NullRealtimePublisher>();
            return;
        }

        var options = ConfigurationOptions.Parse(redisConnectionString);
        // Don't fail startup if Redis is briefly unavailable; the multiplexer reconnects and the
        // implementations swallow transient errors.
        options.AbortOnConnectFail = false;

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options));
        services.AddSingleton<IOrderBookCache, RedisOrderBookCache>();
        services.AddSingleton<IRealtimePublisher, RedisRealtimePublisher>();
    }

    /// <summary>Registers external feed adapters and the factory that resolves them by type.</summary>
    /// <param name="services">The service collection to add to.</param>
    private static void AddExternalFeeds(IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddSingleton<IExternalFeedAdapter, BinanceCryptoAdapter>();
        services.AddSingleton<IExternalFeedAdapter, CustomHttpAdapter>();
        services.AddSingleton<IFeedAdapterFactory, FeedAdapterFactory>();
    }
}
