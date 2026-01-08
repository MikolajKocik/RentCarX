using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Templates;

namespace RentCarX.Presentation.Observability.Loki;

public static class LokiSetup   
{
    /// <summary>
    /// Configures the specified <see cref="WebApplicationBuilder"/> to use Serilog with Grafana Loki as a log sink.
    /// </summary>
    /// <remarks>This method sets up Serilog logging for the application, enriching log events with context
    /// and span information, and writes logs both to the console and to a Grafana Loki instance. Required configuration
    /// values must be present in the application's configuration: <c>otel-exporter-otlp-headers</c>, <c>loki-login</c>,
    /// <c>loki-password</c>, and <c>loki-uri</c>.</remarks>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure. Must not be <c>null</c>.</param>
    /// <exception cref="ArgumentException">Thrown if any of the required configuration values (<c>otel-exporter-otlp-headers</c>, <c>loki-login</c>,
    /// <c>loki-password</c>, or <c>loki-uri</c>) are missing or <c>null</c>.</exception>
    public static void SetLoki(this WebApplicationBuilder builder)
    {
        var login = builder.Configuration["Loki:login"]
            ?? throw new ArgumentException();

        var password = builder.Configuration["Loki:password"]
            ?? throw new ArgumentException();

        var endpoint = builder.Configuration["Loki:otel-exporter-otlp-endpoint"]
            ?? throw new ArgumentException();

        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithSpan()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.GrafanaLoki(
                    uri: endpoint,
                    labels: new[]
                    {
                        new LokiLabel { Key = "app", Value = "RentCarX" },
                        new LokiLabel { Key = "environment", Value = "production" }
                    },
                    credentials: new LokiCredentials
                    {
                        Login = login,
                        Password = password
                    },
                    textFormatter: new ExpressionTemplate("{ {@t, @mt, @l, @x, @r, @tr, @sp, ..@p} }\n")
                );
        });
    }
}
