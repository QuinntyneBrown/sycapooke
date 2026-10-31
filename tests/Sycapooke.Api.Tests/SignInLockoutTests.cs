// Acceptance Test
// Traces to: L2-007
// Description: Five failed sign-ins for one account within 15 minutes lock it for 15 minutes, even
// for the correct password, and the member is emailed. Failures age out and success clears them.

using System.Net;
using Sycapooke.Api.Tests.Support;

namespace Sycapooke.Api.Tests;

public sealed class SignInLockoutTests : IDisposable
{
    private const string Email = "marcus.bell@example.com";
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _client;

    public SignInLockoutTests()
    {
        _client = _factory.CreateMemberClient();
    }

    [Fact]
    public async Task The_sixth_attempt_with_the_correct_password_is_refused_after_five_failures()
    {
        await RegisterAsync();
        await FailAsync(5);

        using var response = await _client.SignInAsync(Email);

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Equal("SIGN_IN_LOCKED", (await Problem.ReadAsync(response)).Code);
    }

    [Fact]
    public async Task Four_failures_do_not_lock_the_account()
    {
        await RegisterAsync();
        await FailAsync(4);

        using var response = await _client.SignInAsync(Email);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task The_lock_ends_after_15_minutes()
    {
        await RegisterAsync();
        await FailAsync(5);
        _factory.Clock.Advance(TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(1));

        using var response = await _client.SignInAsync(Email);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Failures_older_than_15_minutes_do_not_count()
    {
        await RegisterAsync();
        await FailAsync(4);
        _factory.Clock.Advance(TimeSpan.FromMinutes(16));
        await FailAsync(1);

        using var response = await _client.SignInAsync(Email);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task A_successful_sign_in_clears_the_failures()
    {
        await RegisterAsync();
        await FailAsync(4);
        using (var ok = await _client.SignInAsync(Email))
        {
            ok.EnsureSuccessStatusCode();
        }

        await FailAsync(4);

        using var response = await _client.SignInAsync(Email);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Locking_sends_one_account_security_email()
    {
        await RegisterAsync();
        await _factory.DispatchEmailsAsync();
        await FailAsync(5);
        using (await _client.SignInAsync(Email))
        {
        }

        var sent = await _factory.DispatchEmailsAsync();

        var email = Assert.Single(sent);
        Assert.Equal(Email, email.To);
        Assert.Contains("locked", email.Subject, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task An_unknown_email_gets_the_ordinary_failure_and_never_a_lock()
    {
        for (var i = 0; i < 7; i++)
        {
            using var response = await _client.SignInAsync("nobody@example.com", "whatever password");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    private async Task RegisterAsync()
    {
        await _client.RegisterAsync();
        await _client.VerifyAsync(_factory);
    }

    private async Task FailAsync(int count)
    {
        for (var i = 0; i < count; i++)
        {
            using var response = await _client.SignInAsync(Email, "not the password");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
