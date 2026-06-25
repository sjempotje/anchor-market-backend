using System.Text.Json.Serialization;
using AnchorMarket.Application;
using AnchorMarket.Infrastructure;
using AnchorMarket.Infrastructure.Persistence;
using AnchorMarket.Api.Middleware;
using AnchorMarket.Api.WebSockets;
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

// Real-time WebSocket layer. The connection manager is always available; the Redis backplane that
// feeds it is only registered when Redis is configured (otherwise there is nothing to broadcast).
builder.Services.AddSingleton<WebSocketConnectionManager>();
if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Redis")))
    builder.Services.AddHostedService<RealtimeBackplaneService>();

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

// Raw WebSocket endpoint for live price/trade streams. Auth runs via the standard pipeline
// (session token accepted from the ?token= query parameter for the handshake).
app.MapGet("/ws", (HttpContext context, WebSocketConnectionManager manager, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    => RealtimeWebSocketEndpoint.HandleAsync(context, manager, scopeFactory, loggerFactory.CreateLogger("RealtimeWebSocket")));

app.MapControllers();
app.Run();

/// <summary>Application entry point and service configuration.</summary>
public partial class Program { }