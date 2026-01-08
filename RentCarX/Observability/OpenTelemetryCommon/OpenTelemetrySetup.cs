using Grafana.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace RentCarX.Presentation.Observability.OpenTelemetryCommon;

public static class OpenTelemetrySetup
{
    /// <summary>
    /// Configures OpenTelemetry tracing and Serilog self-logging for the specified <see cref="WebApplicationBuilder"/>
    /// instance.
    /// </summary>
    /// <remarks>This method sets up OpenTelemetry tracing with ASP.NET Core, SQL client, and HTTP client
    /// instrumentation, and configures an OTLP exporter using endpoint and header values from the application's
    /// configuration. It also enables Serilog self-logging to the console for diagnostic purposes.</remarks>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure. Must have configuration values for
    /// <c>otel-exporter-otlp-headers</c> and <c>otel-exporter-otlp-endpoint</c>.</param>
    /// <exception cref="ArgumentException">Thrown if the configuration values for <c>otel-exporter-otlp-headers</c> or <c>otel-exporter-otlp-endpoint</c>
    /// are missing or null.</exception>
    public static void Initialize(this WebApplicationBuilder builder)
    {
        Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"[SERILOG DEBUG] {msg}"));

        builder.Services.AddOpenTelemetry()
           .ConfigureResource(r => r.AddService("RentCarX"))
           .WithTracing(tracing => 
               tracing
               .AddSource("RentCarX")
               .AddAspNetCoreInstrumentation()
               .AddSqlClientInstrumentation()
               .AddHttpClientInstrumentation()
           )
           .WithMetrics(metrics =>
               metrics
                .AddMeter("RentCarX")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
           )
           .UseOtlpExporter();
    }
}
