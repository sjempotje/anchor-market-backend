using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnchorMarket.Infrastructure;

public static class DependencyInjection
{
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

        return services;
    }
}
