// Acceptance Test
// Traces to: L2-008
// Description: "Sign out everywhere" revokes every session of the caller on every device and
// leaves other members' sessions alone.

using System.Net;
using Sycapooke.Api.Tests.Support;

namespace Sycapooke.Api.Tests;

public sealed class SignOutEverywhereTests : IDisposable
{
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _laptop;
    private readonly HttpClient _phone;
    private readonly HttpClient _other;

    public SignOutEverywhereTests()
    {
        _laptop = _factory.CreateMemberClient();
        _phone = _factory.CreateMemberClient();
        _other = _factory.CreateMemberClient();
    }

    [Fact]
    public async Task Both_devices_are_signed_out_and_cannot_refresh()
    {
        await _laptop.SignInVerifiedMemberAsync(_factory);
        using (var phoneSignIn = await _phone.SignInAsync())
        {
            phoneSignIn.EnsureSuccessStatusCode();
        }

        using var response = await _laptop.PostAsync("/api/auth/sign-out-everywhere", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var laptopRefresh = await _laptop.PostAsync("/api/auth/refresh", null);
        using var phoneRefresh = await _phone.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, laptopRefresh.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, phoneRefresh.StatusCode);
    }

    [Fact]
    public async Task Another_members_session_is_untouched()
    {
        await _laptop.SignInVerifiedMemberAsync(_factory);
        await _other.SignInVerifiedMemberAsync(_factory, "tessa_b", "tessa.bell@example.com");

        using var response = await _laptop.PostAsync("/api/auth/sign-out-everywhere", null);

        response.EnsureSuccessStatusCode();
        using var otherRefresh = await _other.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.OK, otherRefresh.StatusCode);
    }

    [Fact]
    public async Task A_visitor_is_not_signed_in()
    {
        using var response = await _laptop.PostAsync("/api/auth/sign-out-everywhere", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public void Dispose()
    {
        _laptop.Dispose();
        _phone.Dispose();
        _other.Dispose();
        _factory.Dispose();
    }
}
