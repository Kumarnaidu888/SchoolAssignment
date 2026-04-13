using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Middleware;

namespace SchoolApplication.Hosting;

public static class ApiBehaviorExtensions
{
    public static IServiceCollection AddSchoolApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var correlationId = context.HttpContext.Items[CorrelationIdMiddleware.ItemKey]?.ToString();
                var problem = new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Instance = context.HttpContext.Request.Path,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };

                if (!string.IsNullOrEmpty(correlationId))
                    problem.Extensions["correlationId"] = correlationId;

                return new BadRequestObjectResult(problem);
            };
        });

        return services;
    }
}
