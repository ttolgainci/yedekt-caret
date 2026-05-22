using Microsoft.Extensions.Logging;

namespace MarbleWebProject.Infrastructure;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var id = context.Request.Headers[CorrelationIdDefaults.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(id) && context.Items[CorrelationIdDefaults.HttpContextItemKey] is string existing && !string.IsNullOrWhiteSpace(existing))
            id = existing;
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");

        context.Items[CorrelationIdDefaults.HttpContextItemKey] = id;
        if (!context.Response.HasStarted)
            context.Response.Headers[CorrelationIdDefaults.HeaderName] = id;

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id }))
        {
            CorrelationIdAmbient.SetCurrent(id);
            try
            {
                await _next(context);
            }
            finally
            {
                CorrelationIdAmbient.SetCurrent(null);
            }
        }
    }
}
