using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AnchorMarket.Application;
using AnchorMarket.Infrastructure;
using AnchorMarket.Infrastructure.Persistence;
using AnchorMarket.Api.Middleware;
using AnchorMarket.Api.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.MaxDepth = 128;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 128;
    });
        
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, ctx, _) =>
    {
        doc.Servers = [new() { Url = "http://localhost:5079" }];
        return Task.CompletedTask;
    });

    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.Configure<HostOptions>(o => o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddSingleton<AnchorMarket.Application.Common.Interfaces.IRealtimePublisher, WebSocketRealtimePublisher>();

var app = builder.Build();

app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}

if (builder.Configuration.GetConnectionString("DefaultConnection") is not null)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsEnvironment("Testing"))
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(15) });

app.MapGet("/ws", (HttpContext context, WebSocketConnectionManager manager, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    => RealtimeWebSocketEndpoint.HandleAsync(context, manager, scopeFactory, loggerFactory.CreateLogger("RealtimeWebSocket")))
    .ExcludeFromDescription();

app.MapControllers();
app.Run();

/// <summary>Application entry point and service configuration.</summary>
public partial class Program { }