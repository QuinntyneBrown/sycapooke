// Acceptance Test
// Traces to: L2-009, L2-001, L2-008
// Description: A forgotten password is reset through a single-use emailed link valid for one hour.
// The request never reveals whether an email is registered, at most three links are sent per
// address per hour, and a reset revokes every session and confirms by email.

using System.Net;
using System.Net.Http.Json;
using Sycapooke.Api.Tests.Support;

namespace Sycapooke.Api.Tests;

public sealed class PasswordResetTests : IDisposable
{
    private const string Email = "marcus.bell@example.com";
    private const string NewPassword = "a brand new passphrase";
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _client;

    public PasswordResetTests()
    {
        _client = _factory.CreateMemberClient();
    }

    [Fact]
    public async Task A_registered_email_gets_the_generic_answer_and_one_reset_email()
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        await _factory.DispatchEmailsAsync();

        using var response = await RequestAsync(Email);
        var sent = await _factory.DispatchEmailsAsync();

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var email = Assert.Single(sent);
        Assert.Equal(Email, email.To);
        Assert.Contains("/reset-password?token=", email.Body);
    }

    [Fact]
    public async Task An_unregistered_email_gets_the_same_status_and_no_email()
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        await _factory.DispatchEmailsAsync();

        using var response = await RequestAsync("nobody@example.com");
        var sent = await _factory.DispatchEmailsAsync();

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Empty(sent);
    }

    [Fact]
    public async Task The_fourth_request_in_an_hour_sends_no_further_email()
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        await _factory.DispatchEmailsAsync();

        for (var i = 0; i < 4; i++)
        {
            using var response = await RequestAsync(Email);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        Assert.Equal(3, (await _factory.DispatchEmailsAsync()).Count);
    }

    [Fact]
    public async Task The_link_sets_a_new_password_signs_out_every_session_and_confirms_by_email()
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        var token = await RequestTokenAsync();

        using var reset = await ConfirmAsync(token, NewPassword);
        var sent = await _factory.DispatchEmailsAsync();

        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);
        using var refresh = await _client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
        using var oldPassword = await _client.SignInAsync(Email);
        Assert.Equal(HttpStatusCode.Unauthorized, oldPassword.StatusCode);
        using var newPassword = await _client.SignInAsync(Email, NewPassword);
        Assert.Equal(HttpStatusCode.OK, newPassword.StatusCode);
        Assert.Contains(sent, m => m.Subject.Contains("changed", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task A_link_works_only_once()
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        var token = await RequestTokenAsync();
        using (var first = await ConfirmAsync(token, NewPassword))
        {
            first.EnsureSuccessStatusCode();
        }

        using var second = await ConfirmAsync(token, "another new passphrase");

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        Assert.Equal("RESET_LINK_EXPIRED", (await Problem.ReadAsync(second)).Code);
    }

    [Fact]
    public async Task A_link_older_than_an_hour_is_refused()
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        var token = await RequestTokenAsync();
        _factory.Clock.Advance(TimeSpan.FromMinutes(61));

        using var response = await ConfirmAsync(token, NewPassword);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("RESET_LINK_EXPIRED", (await Problem.ReadAsync(response)).Code);
    }

    [Fact]
    public async Task An_unknown_token_is_refused_the_same_way()
    {
        using var response = await ConfirmAsync("not-a-real-token", NewPassword);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("RESET_LINK_EXPIRED", (await Problem.ReadAsync(response)).Code);
    }

    [Theory]
    [InlineData("short", "PASSWORD_TOO_SHORT")]
    [InlineData("password1234", "PASSWORD_BREACHED")]
    public async Task A_password_that_breaks_the_rules_is_422_and_keeps_the_link_usable(string password, string code)
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        var token = await RequestTokenAsync();

        using var rejected = await ConfirmAsync(token, password);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, rejected.StatusCode);
        Assert.Equal(code, (await Problem.ReadAsync(rejected)).Errors!["password"].Code);
        using var retry = await ConfirmAsync(token, NewPassword);
        Assert.Equal(HttpStatusCode.NoContent, retry.StatusCode);
    }

    [Fact]
    public async Task Resetting_clears_a_lock()
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        for (var i = 0; i < 5; i++)
        {
            using var failed = await _client.SignInAsync(Email, "not the password");
        }

        var token = await RequestTokenAsync();
        using (var reset = await ConfirmAsync(token, NewPassword))
        {
            reset.EnsureSuccessStatusCode();
        }

        using var response = await _client.SignInAsync(Email, NewPassword);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private Task<HttpResponseMessage> RequestAsync(string email) =>
        _client.PostAsJsonAsync("/api/auth/password-reset", new { email });

    private Task<HttpResponseMessage> ConfirmAsync(string token, string password) =>
        _client.PostAsJsonAsync("/api/auth/password-reset/confirm", new { token, password });

    private async Task<string> RequestTokenAsync()
    {
        await _factory.DispatchEmailsAsync();
        using var response = await RequestAsync(Email);
        response.EnsureSuccessStatusCode();
        await _factory.DispatchEmailsAsync();
        return _factory.Email.LastVerificationToken(Email);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
