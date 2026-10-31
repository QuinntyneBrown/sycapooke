// Acceptance Test
// Traces to: L2-048, L2-051, L2-061
// Description: Every API response carries the security headers and no server name. Sign-in and
// registration are throttled per client address with 429 and Retry-After, the window passes with
// time, and health checks are never throttled.

using System.Net;
using System.Net.Http.Json;
using Sycapooke.Api.Tests.Support;

namespace Sycapooke.Api.Tests;

public sealed class HttpHardeningTests : IDisposable
{
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _client;

    public HttpHardeningTests()
    {
        _client = _factory.CreateMemberClient();
    }

    [Theory]
    [InlineData("/api/health")]
    [InlineData("/api/auth/me")]
    [InlineData("/api/does-not-exist")]
    public async Task Every_response_carries_the_security_headers(string path)
    {
        using var response = await _client.GetAsync(path);

        Assert.Equal("default-src 'none'; frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.False(response.Headers.Contains("Server"));
        Assert.False(response.Headers.Contains("X-Powered-By"));
    }

    [Fact]
    public async Task Too_many_sign_in_attempts_from_one_address_get_429_with_retry_after_until_the_window_passes()
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            using var allowed = await _client.PostAsJsonAsync("/api/auth/sign-in", new { email = "nobody@example.com", password = "wrong password here" });
            Assert.NotEqual(HttpStatusCode.TooManyRequests, allowed.StatusCode);
        }

        using var limited = await _client.PostAsJsonAsync("/api/auth/sign-in", new { email = "nobody@example.com", password = "wrong password here" });

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.True(int.Parse(limited.Headers.GetValues("Retry-After").Single()) > 0);
        Assert.Equal("RATE_LIMITED", (await Problem.ReadAsync(limited)).Code);

        _factory.Clock.Advance(TimeSpan.FromMinutes(16));
        using var again = await _client.PostAsJsonAsync("/api/auth/sign-in", new { email = "nobody@example.com", password = "wrong password here" });
        Assert.NotEqual(HttpStatusCode.TooManyRequests, again.StatusCode);
    }

    [Fact]
    public async Task Registration_is_limited_to_five_an_hour_and_health_checks_are_never_throttled()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var allowed = await _client.PostAsJsonAsync("/api/auth/register", new { });
            Assert.NotEqual(HttpStatusCode.TooManyRequests, allowed.StatusCode);
        }

        using var limited = await _client.PostAsJsonAsync("/api/auth/register", new { });
        for (var i = 0; i < 400; i++)
        {
            using var health = await _client.GetAsync("/api/health");
            Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
