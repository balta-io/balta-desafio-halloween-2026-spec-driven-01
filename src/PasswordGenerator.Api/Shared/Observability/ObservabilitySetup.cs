using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace PasswordGenerator.Api.Shared.Observability;

public static class ObservabilitySetup
{
    public static IHostBuilder AddStructuredLogging(this IHostBuilder host) =>
        host.UseSerilog((context, services, logger) =>
        {
            logger
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.With<RawPathRemover>()
                .WriteTo.Console(new CompactJsonFormatter());

            foreach (var sink in services.GetServices<ILogEventSink>())
                logger.WriteTo.Sink(sink);
        }, preserveStaticLogger: true);

    public static IApplicationBuilder UseRouteTemplateRequestLogging(this IApplicationBuilder app) =>
        app.UseSerilogRequestLogging(options =>
        {
            options.Logger = app.ApplicationServices.GetRequiredService<Serilog.ILogger>();
            options.MessageTemplate = "HTTP {RequestMethod} {RouteTemplate} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, http) =>
                diagnosticContext.Set("RouteTemplate", (http.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "unmatched");
        });

    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        services.AddSingleton<PasswordMetrics>();
        services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");
        return services;
    }

    private sealed class RawPathRemover : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) =>
            logEvent.RemovePropertyIfPresent("RequestPath");
    }
}
