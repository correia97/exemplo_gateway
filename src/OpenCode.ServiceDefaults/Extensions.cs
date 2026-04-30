using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Npgsql;

public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.Services.AddOpenApi();

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        AppContext.SetSwitch("Npgsql.EnableTelemetry", true);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddRuntimeInstrumentation()
                       .AddNpgsqlInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddNpgsql();
            });

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
        return app;
    }

    public static WebApplication UseCorrelationId(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }
}

internal class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = Guid.NewGuid().ToString();

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        context.TraceIdentifier = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdHeader] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
