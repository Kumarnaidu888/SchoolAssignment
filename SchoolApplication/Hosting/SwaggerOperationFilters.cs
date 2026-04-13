using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SchoolApplication.Hosting;

/// <summary>Only adds Bearer to operations that are not anonymous (so login does not send a JWT).</summary>
public sealed class BearerAuthOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var allowAnonymous = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .Any(m => m is IAllowAnonymous);

        if (allowAnonymous)
        {
            operation.Security = [];
            return;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = []
            }
        ];
    }
}

/// <summary>Clarifies login body: Swagger's default "string" placeholders are not valid credentials.</summary>
public sealed class LoginRequestExampleFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.RelativePath?.Equals("api/auth/login", StringComparison.OrdinalIgnoreCase) != true)
            return;
        if (!string.Equals(context.ApiDescription.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            return;

        operation.Summary ??= "Login";
        operation.Description =
            "**No Bearer token here.** After a successful login, use **Authorize** (lock icon) and paste `Bearer {accessToken}` for other endpoints. " +
            "Replace the example JSON with a real `userName` and `password` from your `auth.AppUsers` table (password must be a BCrypt hash).";

        operation.RequestBody ??= new OpenApiRequestBody { Required = true };
        operation.RequestBody.Content["application/json"] = new OpenApiMediaType
        {
            Example = new OpenApiObject
            {
                ["userName"] = new OpenApiString("admin"),
                ["password"] = new OpenApiString("Use-your-real-password")
            }
        };
    }
}
