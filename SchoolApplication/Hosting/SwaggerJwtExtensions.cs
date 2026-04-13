using Microsoft.OpenApi.Models;

namespace SchoolApplication.Hosting;

public static class SwaggerJwtExtensions
{
    public static IServiceCollection AddSchoolSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "School Assessment API",
                Version = "v1",
                Description =
                    "Student assessment & ranking: JWT auth, classes/sections/students, teacher assignments, async marks jobs, rankings (competition/tie rules)."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description =
                    "Paste: `Bearer {accessToken}` using the **accessToken** value from `POST /api/auth/login`. " +
                    "Do **not** set this when calling login/refresh/logout.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            // Do not add a global security requirement — it would attach Bearer to /api/auth/login and confuse Swagger.
            options.OperationFilter<BearerAuthOperationFilter>();
            options.OperationFilter<LoginRequestExampleFilter>();
        });

        return services;
    }
}
