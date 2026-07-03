using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Infrastructure.Auth;
using AnchorMarket.Infrastructure.BackgroundServices;
using AnchorMarket.Infrastructure.BackgroundServices.BotSimulation;
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
        var hasDatabase = !string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection"));
        if (!enabled || !hasDatabase)
            return;

        services.AddHostedService<VolumeStatsUpdaterService>();
        services.AddHostedService<PartitionManagerService>();

        AddBotSimulation(services, configuration);
    }

    /// <summary>
    /// Registers the bot-simulation subsystem (auto-created public markets + trading bots) when
    /// "BotSimulation:Enabled" is true. Intended for development and demo environments to make the
    /// platform look busy; left off by default so it never runs in production or tests.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The application configuration.</param>
    private static void AddBotSimulation(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(BotSimulationOptions.SectionName);
        services.Configure<BotSimulationOptions>(section);

        if (!section.GetValue<bool>(nameof(BotSimulationOptions.Enabled)))
            return;

        services.AddSingleton<BotSimulationSeeder>();
        services.AddSingleton<BotTradeExecutor>();
        services.AddHostedService<MarketFactoryService>();
        services.AddHostedService<BotTradingService>();
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
