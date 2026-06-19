using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace AnchorMarket.Infrastructure.Persistence;

/// <summary>Design-time factory for creating <see cref="ApplicationDbContext"/> instances during EF Core CLI operations.</summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <summary>Creates a new <see cref="ApplicationDbContext"/> instance using configuration from the Api project.</summary>
    /// <param name="args">Command-line arguments passed by the EF Core CLI.</param>
    /// <returns>A configured <see cref="ApplicationDbContext"/> instance.</returns>
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var apiSettings = Path.Combine(basePath, "..", "AnchorMarket.Api");

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true);

        if (Directory.Exists(apiSettings))
        {
            builder
                .AddJsonFile(Path.Combine(apiSettings, "appsettings.json"), optional: true)
                .AddJsonFile(Path.Combine(apiSettings, "appsettings.Development.json"), optional: true);
        }

        // Load user secrets from the Api project so EF CLI picks up the connection string locally
        builder.AddUserSecrets("099d1540-3925-4bf2-9eaa-eba3a88558e6");

        var configuration = builder.AddEnvironmentVariables().Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
