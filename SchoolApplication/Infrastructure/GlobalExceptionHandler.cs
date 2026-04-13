using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SchoolApplication.Exceptions;
using SchoolApplication.Middleware;

namespace SchoolApplication.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Items[CorrelationIdMiddleware.ItemKey]?.ToString();

        (int status, string title, string detail, IDictionary<string, string[]>? errors) = exception switch
        {
            ValidationException vex =>
                (StatusCodes.Status400BadRequest,
                "Validation failed",
                "One or more validation errors occurred.",
                vex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),

            NotFoundException nf =>
                (StatusCodes.Status404NotFound, "Not found", nf.Message, null),

            AuthenticationException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized", "Invalid or missing credentials.", null),

            ForbiddenException fe =>
                (StatusCodes.Status403Forbidden, "Forbidden", fe.Message, null),

            ConflictException ce =>
                (StatusCodes.Status409Conflict, "Conflict", ce.Message, null),

            UnauthorizedAccessException ua =>
                (StatusCodes.Status403Forbidden, "Forbidden", ua.Message, null),

            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) =>
                (StatusCodes.Status409Conflict,
                "Conflict",
                "The request could not be completed because it would create a duplicate record.",
                null),

            DbUpdateException dbEx when IsReferentialConstraintViolation(dbEx) =>
                (StatusCodes.Status409Conflict,
                "Conflict",
                "The operation failed because related data exists or a referenced record is missing.",
                null),

            _ => (StatusCodes.Status500InternalServerError,
                "Server error",
                _environment.IsDevelopment() ? exception.ToString() : "An unexpected error occurred.",
                null)
        };

        var level = status >= 500 ? LogLevel.Error : LogLevel.Warning;
        _logger.Log(level, exception,
            "Request failed with {StatusCode}. Title: {Title}. CorrelationId: {CorrelationId}",
            status, title, correlationId);

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = errors is null ? detail : null,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.io/{status}"
        };

        if (correlationId is not null)
            problem.Extensions["correlationId"] = correlationId;

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return FindSqlException(ex)?.Number is 2601 or 2627;
    }

    private static bool IsReferentialConstraintViolation(DbUpdateException ex)
    {
        return FindSqlException(ex)?.Number == 547;
    }

    private static SqlException? FindSqlException(Exception ex)
    {
        for (var e = ex; e is not null; e = e.InnerException)
        {
            if (e is SqlException sql)
                return sql;
        }

        return null;
    }
}
