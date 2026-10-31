// Acceptance Test
// Traces to: L2-001, L2-008, L2-009
// Description: A signed-in member changes their password by proving the current one. Other devices
// are signed out, this device stays signed in, and a confirmation email is queued.

using System.Net;
using System.Net.Http.Json;
using Sycapooke.Api.Tests.Support;

namespace Sycapooke.Api.Tests;

public sealed class ChangePasswordTests : IDisposable
{
    private const string NewPassword = "a brand new passphrase";
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _laptop;
    private readonly HttpClient _phone;

    public ChangePasswordTests()
    {
        _laptop = _factory.CreateMemberClient();
        _phone = _factory.CreateMemberClient();
    }

    [Fact]
    public async Task The_new_password_works_other_devices_are_signed_out_and_this_one_stays_in()
    {
        await _laptop.SignInVerifiedMemberAsync(_factory);
        using (var phoneSignIn = await _phone.SignInAsync())
        {
            phoneSignIn.EnsureSuccessStatusCode();
        }

        await _factory.DispatchEmailsAsync();

        using var response = await ChangeAsync(Members.Password, NewPassword);
        var sent = await _factory.DispatchEmailsAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var phoneRefresh = await _phone.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, phoneRefresh.StatusCode);
        using var laptopMe = await _laptop.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, laptopMe.StatusCode);
        using var laptopRefresh = await _laptop.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.OK, laptopRefresh.StatusCode);
        using var signIn = await _phone.SignInAsync(password: NewPassword);
        Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
        Assert.Contains(sent, m => m.Subject.Contains("changed", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task A_wrong_current_password_is_422_and_changes_nothing()
    {
        await _laptop.SignInVerifiedMemberAsync(_factory);

        using var response = await ChangeAsync("not my password", NewPassword);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("CURRENT_PASSWORD_INCORRECT", (await Problem.ReadAsync(response)).Errors!["currentPassword"].Code);
        using var signIn = await _phone.SignInAsync();
        Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
    }

    [Fact]
    public async Task A_too_short_new_password_is_422()
    {
        await _laptop.SignInVerifiedMemberAsync(_factory);

        using var response = await ChangeAsync(Members.Password, "short");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("PASSWORD_TOO_SHORT", (await Problem.ReadAsync(response)).Errors!["newPassword"].Code);
    }

    [Fact]
    public async Task A_visitor_is_401()
    {
        using var response = await ChangeAsync(Members.Password, NewPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private Task<HttpResponseMessage> ChangeAsync(string currentPassword, string newPassword) =>
        _laptop.PostAsJsonAsync("/api/account/password", new { currentPassword, newPassword });

    public void Dispose()
    {
        _laptop.Dispose();
        _phone.Dispose();
        _factory.Dispose();
    }
}
