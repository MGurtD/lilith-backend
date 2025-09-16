using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, $"Exception: Unhandled exception in {context.Request.Path} after {stopwatch.ElapsedMilliseconds}ms");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            var errorResponse = new { error = "Internal server error" };
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            return;
        }

        stopwatch.Stop();
        _logger.LogInformation($"Exception: {context.Request.Method} {context.Request.Path} executed in {stopwatch.ElapsedMilliseconds}ms");
    }
}
