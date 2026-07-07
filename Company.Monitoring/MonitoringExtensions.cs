using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace Company.Monitoring;

/// <summary>
/// Centralized Prometheus metrics + health check configuration shared
/// across all company projects. UseHttpMetrics/MapMetrics automatically
/// expose HTTP metrics plus default process/.NET runtime metrics
/// (CPU, memory, GC, thread count) with zero extra config.
/// </summary>
public static class MonitoringExtensions
{
    public static IServiceCollection AddCompanyMonitoring(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' was not found in configuration. " +
                "Company monitoring requires a valid Postgres connection string for health checks.");
        }

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgres");

        return services;
    }

    public static WebApplication UseCompanyMonitoring(this WebApplication app)
    {
        // Tracks request/response body sizes (custom middleware)
        app.UseMiddleware<RequestResponseSizeMiddleware>();

        // Tracks request count, duration, in-progress count, method, status code
        app.UseHttpMetrics();

        // Exposes everything (HTTP + process + GC metrics) at /metrics
        app.MapMetrics();

        // Exposes health check results at /health
        app.MapHealthChecks("/health");

        return app;
    }
}