using Microsoft.AspNetCore.Http;
using Prometheus;

namespace Company.Monitoring;

/// <summary>
/// Tracks HTTP request and response body sizes as Prometheus histograms.
/// Not provided by default in prometheus-net.AspNetCore, so we add it here
/// once and every project referencing Company.Monitoring gets it for free.
/// </summary>
public class RequestResponseSizeMiddleware
{
    private static readonly Histogram RequestSize = Metrics.CreateHistogram(
        "http_request_size_bytes",
        "Size of incoming HTTP requests in bytes",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(100, 2, 10),
            LabelNames = new[] { "method", "path" }
        });

    private static readonly Histogram ResponseSize = Metrics.CreateHistogram(
        "http_response_size_bytes",
        "Size of outgoing HTTP responses in bytes",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(100, 2, 10),
            LabelNames = new[] { "method", "path" }
        });

    private readonly RequestDelegate _next;

    public RequestResponseSizeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "unknown";

        if (context.Request.ContentLength.HasValue)
        {
            RequestSize.WithLabels(method, path).Observe(context.Request.ContentLength.Value);
        }

        var originalBodyStream = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await _next(context);

        ResponseSize.WithLabels(method, path).Observe(memStream.Length);

        memStream.Position = 0;
        await memStream.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }
}