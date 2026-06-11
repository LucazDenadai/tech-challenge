using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OficinaMecanica.Estoque.API.Extensions;

public static class OpenTelemetryExtensions
{
    public static IHostApplicationBuilder AddOpenTelemetry(
        this IHostApplicationBuilder builder, string serviceName)
    {
        var jaegerEndpoint = builder.Configuration["Jaeger__Endpoint"] ?? "http://jaeger:4317";

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddEntityFrameworkCoreInstrumentation(o => o.SetDbStatementForText = true)
                .AddMassTransitInstrumentation()
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(jaegerEndpoint);
                    o.Protocol = OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter());

        return builder;
    }
}
