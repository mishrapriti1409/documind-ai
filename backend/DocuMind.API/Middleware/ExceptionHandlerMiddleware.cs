using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DocuMind.API.Middleware;

public sealed class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
            await WriteErrorAsync(context, 401, "Unauthorized", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            await WriteErrorAsync(context, 404, "Not Found", ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            await WriteErrorAsync(context, 400, "Bad Request", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation: {Message}", ex.Message);
            await WriteErrorAsync(context, 400, "Bad Request", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled server error on {Method} {Path}", context.Request.Method, context.Request.Path);
            var detail = _env.IsDevelopment() ? ex.ToString() : "An unexpected server error occurred.";
            await WriteErrorAsync(context, 500, "Internal Server Error", detail);
        }
    }

    private static async Task WriteErrorAsync(HttpContext ctx, int status, string title, string detail)
    {
        if (ctx.Response.HasStarted) return;

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = ctx.Request.Path
        };
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}

