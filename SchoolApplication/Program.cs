using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolApplication.Hosting;
using SchoolApplication.Infrastructure;
using SchoolApplication.Middleware;
using SchoolApplication.Models;
using SchoolApplication.Options;
using SchoolApplication.Security;
using SchoolApplication.Services;
using SchoolApplication.Services.Auth;
using SchoolApplication.Services.Processing;
using SchoolApplication.Workers;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName();
    });

    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

    builder.Services.AddSchoolApiBehavior();
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
    if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
        throw new InvalidOperationException("Configure Jwt:SigningKey in appsettings (min 32 characters).");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                ClockSkew = TimeSpan.FromMinutes(1),
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
            };

            // Ignore Authorization header on auth routes so a stale Swagger "Authorize" token cannot break login.
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Path.Value?.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase) == true)
                        context.Token = null;
                    return Task.CompletedTask;
                }
            };
        });
    builder.Services.AddAuthorization();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSchoolSwaggerWithJwt();

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();

    builder.Services.AddDbContext<SchoolAssessmentContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolAssessment")));

    builder.Services.AddScoped<IClassService, ClassService>();
    builder.Services.AddScoped<ISectionService, SectionService>();
    builder.Services.AddScoped<IStudentService, StudentService>();
    builder.Services.AddScoped<ITeacherAssignmentService, TeacherAssignmentService>();
    builder.Services.AddScoped<IReferenceDataService, ReferenceDataService>();
    builder.Services.AddScoped<IMarkSubmissionService, MarkSubmissionService>();
    builder.Services.AddScoped<IMarkJobQueryService, MarkJobQueryService>();
    builder.Services.AddScoped<IMarkJobProcessor, MarkJobProcessor>();
    builder.Services.AddScoped<IRankingQueryService, RankingQueryService>();
    builder.Services.AddScoped<IMePortalService, MePortalService>();
    builder.Services.AddScoped<ICurrentUserProfileService, CurrentUserProfileService>();
    builder.Services.AddScoped<ISectionMarkReadService, SectionMarkReadService>();
    builder.Services.AddSingleton<ITokenIssuer, TokenIssuer>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();

    builder.Services.AddHostedService<MarkProcessingWorker>();

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseExceptionHandler();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var correlationId = httpContext.Items[CorrelationIdMiddleware.ItemKey]?.ToString();
            if (!string.IsNullOrEmpty(correlationId))
                diagnosticContext.Set("CorrelationId", correlationId);
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "School Assessment API v1");
        });
    }

    // In Development, HTTPS redirection breaks Swagger "Try it out" on http://localhost when the
    // dev HTTPS certificate is not trusted (browser shows "Failed to fetch" after a 307 to https).
    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
