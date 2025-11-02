using Domain.Entities.Audit;
using Infrastructure.Persistance;
using System.Diagnostics;

public class HttpLoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        var stopwatch = Stopwatch.StartNew();
        await next(context);
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

        if (dbContext != null && dbContext.HttpTransactionLogs != null)
        {
            dbContext.HttpTransactionLogs.Add(log);
            await dbContext.SaveChangesAsync();
        }
    }
}
