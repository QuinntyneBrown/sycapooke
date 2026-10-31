// Acceptance Test
// Traces to: L2-004, L2-010
// Description: A customer reads and renames only their own private account. Names follow the
// registration rules and are unique ignoring case; no request field changes privileges.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Sycapooke.Api.Tests.Support;

namespace Sycapooke.Api.Tests;

public sealed class PrivateAccountTests : IDisposable
{
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _client;

    public PrivateAccountTests()
    {
        _client = _factory.CreateMemberClient();
    }

    [Fact]
    public async Task The_account_shows_only_the_callers_private_details()
    {
        await _client.SignInVerifiedMemberAsync(_factory);

        var account = await _client.GetFromJsonAsync<JsonElement>("/api/account");

        Assert.Equal("marcus_12", account.GetProperty("displayName").GetString());
        Assert.Equal("marcus.bell@example.com", account.GetProperty("email").GetString());
        Assert.True(account.GetProperty("emailVerified").GetBoolean());
        Assert.Equal(DateOnly.FromDateTime(_factory.Clock.GetUtcNow().UtcDateTime).ToString("yyyy-MM-dd"), account.GetProperty("memberSince").GetString());
        Assert.Equal(4, account.EnumerateObject().Count());
    }

    [Fact]
    public async Task A_valid_new_name_is_saved()
    {
        await _client.SignInVerifiedMemberAsync(_factory);

        using var response = await _client.PatchAsJsonAsync("/api/account", new { displayName = "  marcus_bell  " });

        response.EnsureSuccessStatusCode();
        var account = await _client.GetFromJsonAsync<JsonElement>("/api/account");
        Assert.Equal("marcus_bell", account.GetProperty("displayName").GetString());
        var me = await _client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        Assert.Equal("marcus_bell", me.GetProperty("displayName").GetString());
    }

    [Fact]
    public async Task The_unchanged_name_and_a_different_case_of_it_are_valid()
    {
        await _client.SignInVerifiedMemberAsync(_factory);

        using var same = await _client.PatchAsJsonAsync("/api/account", new { displayName = "marcus_12" });
        using var recased = await _client.PatchAsJsonAsync("/api/account", new { displayName = "MARCUS_12" });

        Assert.Equal(HttpStatusCode.OK, same.StatusCode);
        Assert.Equal(HttpStatusCode.OK, recased.StatusCode);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("this_name_is_way_too_long_to_be_ok")]
    [InlineData("has space")]
    [InlineData("")]
    public async Task A_badly_formed_name_is_422_with_a_field_error(string name)
    {
        await _client.SignInVerifiedMemberAsync(_factory);

        using var response = await _client.PatchAsJsonAsync("/api/account", new { displayName = name });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("DISPLAY_NAME_FORMAT", (await Problem.ReadAsync(response)).Errors!["displayName"].Code);
    }

    [Theory]
    [InlineData("tessa_b")]
    [InlineData("TESSA_B")]
    public async Task A_name_taken_by_someone_else_ignoring_case_is_422(string name)
    {
        using var other = _factory.CreateMemberClient();
        await other.SignInVerifiedMemberAsync(_factory, "tessa_b", "tessa.bell@example.com");
        await _client.SignInVerifiedMemberAsync(_factory);

        using var response = await _client.PatchAsJsonAsync("/api/account", new { displayName = name });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("DISPLAY_NAME_TAKEN", (await Problem.ReadAsync(response)).Errors!["displayName"].Code);
    }

    [Fact]
    public async Task Forged_privilege_fields_change_nothing()
    {
        await _client.SignInVerifiedMemberAsync(_factory);

        using var response = await _client.PatchAsJsonAsync("/api/account", new { displayName = "marcus_12", isFounder = true, memberId = Guid.NewGuid(), isAdministrator = true });

        response.EnsureSuccessStatusCode();
        var me = await _client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        Assert.False(me.GetProperty("isFounder").GetBoolean());
    }

    [Fact]
    public async Task A_visitor_is_401()
    {
        using var get = await _client.GetAsync("/api/account");
        using var patch = await _client.PatchAsJsonAsync("/api/account", new { displayName = "someone" });

        Assert.Equal(HttpStatusCode.Unauthorized, get.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, patch.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
