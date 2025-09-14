using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BinaryMessageEncodingAPI.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (ValidationException vex)
        {
            _logger.LogWarning(vex, "Validation error");
            await WriteProblem(ctx, StatusCodes.Status400BadRequest, vex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error");
            await WriteProblem(ctx, StatusCodes.Status500InternalServerError, "Unexpected error.");
        }
    }

    private static Task WriteProblem(HttpContext ctx, int status, string detail)
    {
        var pd = new ProblemDetails
        {
            Status = status,
            Title = $"HTTP {status}",
            Detail = detail
        };
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";
        return ctx.Response.WriteAsJsonAsync(pd);
    }
}
