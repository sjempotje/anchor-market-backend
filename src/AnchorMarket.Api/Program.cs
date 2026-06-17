using System.Text.Json.Serialization;
using AnchorMarket.Application;
using AnchorMarket.Infrastructure;
using AnchorMarket.Infrastructure.Persistence;
using AnchorMarket.Api.Middleware;
using Scalar.AspNetCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
        
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, ctx, _) =>
    {
        doc.Servers = [new() { Url = "http://localhost:5079" }];
        return Task.CompletedTask;
    });

    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

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

app.MapControllers();
app.Run();

public partial class Program { }