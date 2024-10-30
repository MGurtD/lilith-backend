﻿using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Infrastructure.Persistance;
using Domain.Entities.Audit;

public class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public HttpLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        var log = new HttpTransactionLog
        {
            Method = context.Request.Method,
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.ToString(),
            StatusCode = context.Response.StatusCode,
            Timestamp = DateTime.UtcNow,
            Duration = stopwatch.ElapsedMilliseconds
        };

        dbContext.HttpTransactionLogs.Add(log);
        await dbContext.SaveChangesAsync();
    }
}
