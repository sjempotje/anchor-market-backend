using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Infrastructure.Auth;
using AnchorMarket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnchorMarket.Infrastructure;

public static class DependencyInjection
{
    public const string SchemeName = "BetterAuth";

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
