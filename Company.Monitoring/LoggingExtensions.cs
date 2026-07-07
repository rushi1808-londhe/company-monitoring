using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Company.Monitoring;

/// <summary>
/// Centralized Serilog configuration shared across all company projects.
/// Any fix or improvement made here applies automatically to every
/// project referencing this library.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog as the application's logging provider.
    /// Call this immediately after creating the WebApplicationBuilder.
    /// </summary>
    public static void ConfigureCompanyLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
