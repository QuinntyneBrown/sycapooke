// Acceptance Test
// Traces to: L2-003, L2-010
// Description: The server derives isFounder from the configured founder ID agreeing with the stored
// singleton binding, for an open, verified account. Nothing a client sends changes it.

using System.Net.Http.Json;
using System.Text.Json;
using Sycapooke.Api.Tests.Support;

namespace Sycapooke.Api.Tests;

public sealed class FounderAccessTests : IDisposable
{
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _client;

    public FounderAccessTests()
    {
        _client = _factory.CreateMemberClient();
    }

    [Fact]
    public async Task An_ordinary_verified_member_is_not_the_founder()
    {
        await _client.SignInVerifiedMemberAsync(_factory);

        Assert.False(await IsFounderAsync());
    }

    [Fact]
    public async Task The_bound_and_configured_founder_is_the_founder()
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        _factory.BindFounder(_factory.MemberId("marcus.bell@example.com"));

        Assert.True(await IsFounderAsync());
    }

    [Fact]
    public async Task A_configured_id_without_a_stored_binding_grants_nothing()
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        _factory.Shop.FounderMemberId = _factory.MemberId("marcus.bell@example.com");

        Assert.False(await IsFounderAsync());
    }

    [Fact]
    public async Task A_stored_binding_that_disagrees_with_the_configuration_grants_nothing()
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        var memberId = _factory.MemberId("marcus.bell@example.com");
        _factory.BindFounder(memberId);
        _factory.Shop.FounderMemberId = Guid.NewGuid();

        Assert.False(await IsFounderAsync());
    }

    [Fact]
    public async Task Missing_configuration_grants_nothing()
    {
        await _client.SignInVerifiedMemberAsync(_factory);
        _factory.BindFounder(_factory.MemberId("marcus.bell@example.com"));
        _factory.Shop.FounderMemberId = null;

        Assert.False(await IsFounderAsync());
    }

    private async Task<bool> IsFounderAsync()
    {
        var me = await _client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        return me.GetProperty("isFounder").GetBoolean();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
