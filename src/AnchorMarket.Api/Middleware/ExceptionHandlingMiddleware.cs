using System;
using System.Linq;
using AnchorMarket.Application.Common.Exceptions;
using FluentValidation;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AnchorMarket.Api.Middleware;

/// <summary>Catches unhandled exceptions and returns structured error responses.</summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>Invokes the middleware, mapping exceptions to HTTP status codes.</summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var body = JsonSerializer.Serialize(new { errors });
            await context.Response.WriteAsync(body);
        }
        catch (ForbiddenException ex)
        {
            logger.LogWarning(ex, "Forbidden");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "Not found");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
        catch (InvalidOperationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            var body = JsonSerializer.Serialize(new { error = ex.Message });
            await context.Response.WriteAsync(body);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
