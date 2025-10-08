using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class TimingActionFilter : IAsyncActionFilter
{
    private readonly ILogger<TimingActionFilter> _logger;

    public TimingActionFilter(ILogger<TimingActionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sw = Stopwatch.StartNew();

        var resultContext = await next();

        sw.Stop();
        var actionName = context.ActionDescriptor.DisplayName;
        _logger.LogInformation($"Timing: Action {actionName} took {sw.ElapsedMilliseconds}ms");
    }
}
