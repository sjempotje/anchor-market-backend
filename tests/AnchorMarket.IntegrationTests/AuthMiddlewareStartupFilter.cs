using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace AnchorMarket.IntegrationTests;

public class AuthMiddlewareStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseAuthentication();
            app.UseAuthorization();
            next(app);
        };
    }
}
