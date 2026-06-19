using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

/// <summary>Adds Bearer security scheme to OpenAPI for endpoints requiring authorization.</summary>
internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    /// <summary>Transforms the OpenAPI document to include bearer authentication.</summary>
    /// <param name="document">The OpenAPI document to transform.</param>
    /// <param name="context">The transformer context providing API description groups.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (!authenticationSchemes.Any(s => s.Name is "Bearer" or "BetterAuth"))
            return;

        var bearerScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            In = ParameterLocation.Header,
            BearerFormat = "session token"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = bearerScheme
        };

        document.Workspace ??= new OpenApiWorkspace();
        document.Workspace.RegisterComponentForDocument(document, bearerScheme, "Bearer");

        var descriptionLookup = context.DescriptionGroups
            .SelectMany(g => g.Items)
            .Where(d => d.RelativePath is not null && d.HttpMethod is not null)
            .ToDictionary(
                d => ("/" + d.RelativePath!.TrimEnd('/'), d.HttpMethod!.ToUpperInvariant()),
                d => d.ActionDescriptor.EndpointMetadata);

        foreach (var (path, pathItem) in document.Paths)
        {
            foreach (var (operationType, operation) in pathItem.Operations)
            {
                if (!descriptionLookup.TryGetValue((path, operationType.ToString().ToUpperInvariant()), out var metadata))
                    continue;
                if (metadata.OfType<IAllowAnonymous>().Any() || !metadata.OfType<IAuthorizeData>().Any())
                    continue;

                operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
                operation.Security = [new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                }];
            }
        }
    }
}
