using System.Collections.Concurrent;
using System.Globalization;
using Sycapooke.Application.Common;

namespace Sycapooke.Api.Http;

/// <summary>
/// Writes the API's response headers on every response, errors and health checks included: no
/// content may load from an API response, nothing may frame it, and the server name is removed.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers.Remove("Server");
            headers.Remove("X-Powered-By");
            return Task.CompletedTask;
        });
        return next(context);
    }
}

/// <summary>Answers an unhandled exception with problem details that carry the correlation ID and no stack trace.</summary>
public static class ExceptionHandling
{
    public static async Task WriteAsync(HttpContext context)
    {
        await ProblemResults.WriteAsync(context, new Error(500, "INTERNAL", "Something went wrong"));
    }
}

/// <summary>One named limit: this many requests per window, counted per partition key.</summary>
public sealed record RateLimitPolicy(string Name, int Limit, TimeSpan Window);

/// <summary>The result of asking for a request: allowed, or how long until the window ends.</summary>
public readonly record struct RateLimitLease(bool Allowed, TimeSpan RetryAfter);

/// <summary>Counts requests per key and window. A shared store such as Redis replaces the in-memory one when the API scales out.</summary>
public interface IRateLimitStore
{
    RateLimitLease Acquire(string key, RateLimitPolicy policy, DateTimeOffset now);
}

/// <summary>A fixed-window counter in this process's memory; fine for one instance and for tests.</summary>
public sealed class InMemoryRateLimitStore : IRateLimitStore
{
    private readonly ConcurrentDictionary<string, (DateTimeOffset WindowEnds, int Count)> _windows = new();

    public RateLimitLease Acquire(string key, RateLimitPolicy policy, DateTimeOffset now)
    {
        var entry = _windows.AddOrUpdate(
            key,
            _ => (now + policy.Window, 1),
            (_, current) => current.WindowEnds <= now ? (now + policy.Window, 1) : (current.WindowEnds, current.Count + 1));
        return entry.Count <= policy.Limit ? new RateLimitLease(true, TimeSpan.Zero) : new RateLimitLease(false, entry.WindowEnds - now);
    }
}

/// <summary>The limits (platform/harden-http-and-api): a default per client address, and stricter ones for registration and sign-in.</summary>
public static class RateLimitPolicies
{
    public static readonly RateLimitPolicy Default = new("default", 300, TimeSpan.FromMinutes(1));
    public static readonly RateLimitPolicy Register = new("register", 5, TimeSpan.FromHours(1));
    public static readonly RateLimitPolicy SignIn = new("sign-in", 20, TimeSpan.FromMinutes(15));

    /// <summary>The extra policy of a path, if any.</summary>
    public static RateLimitPolicy? For(HttpRequest request)
    {
        if (!HttpMethods.IsPost(request.Method))
        {
            return null;
        }

        return request.Path.Value?.TrimEnd('/').ToLowerInvariant() switch
        {
            "/api/auth/register" => Register,
            "/api/auth/password-reset" => Register,
            "/api/auth/sign-in" => SignIn,
            _ => null,
        };
    }
}

/// <summary>Answers 429 with Retry-After over the limit. Health checks and the payment webhook are exempt.</summary>
public sealed class RateLimitingMiddleware(RequestDelegate next, IRateLimitStore store, TimeProvider clock)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/api/health", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/api/payments/webhook", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var client = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = clock.GetUtcNow();
        var lease = store.Acquire($"rl:{RateLimitPolicies.Default.Name}:{client}", RateLimitPolicies.Default, now);
        if (lease.Allowed && RateLimitPolicies.For(context.Request) is { } extra)
        {
            lease = store.Acquire($"rl:{extra.Name}:{client}", extra, now);
        }

        if (lease.Allowed)
        {
            await next(context);
            return;
        }

        context.Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(lease.RetryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        await ProblemResults.WriteAsync(context, new Error(429, "RATE_LIMITED", "Too many requests"));
    }
}
