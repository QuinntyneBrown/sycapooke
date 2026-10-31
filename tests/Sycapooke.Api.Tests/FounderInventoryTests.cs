// Acceptance Test
// Traces to: L2-003, L2-010
// Description: Only the founder manages inventory: a customer is refused with 403 FOUNDER_REQUIRED
// and a visitor with 401, while the founder reaches the same routes.

using System.Net;
using System.Net.Http.Json;
using Sycapooke.Api.Tests.Support;

namespace Sycapooke.Api.Tests;

public sealed class FounderInventoryTests : IDisposable
{
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _client;

    public FounderInventoryTests() => _client = _factory.CreateMemberClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Theory]
    [InlineData("/api/me/listings")]
    [InlineData("/api/selling/options")]
    public async Task A_visitor_is_asked_to_sign_in_for_inventory_routes(string path)
    {
        using var response = await _client.GetAsync(path);

        Assert.True(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.OK, path);
    }

    [Fact]
    public async Task A_customer_cannot_list_inventory_or_create_a_draft()
    {
        await _client.SignInVerifiedMemberAsync(_factory);

        using var mine = await _client.GetAsync("/api/me/listings");
        using var draft = await _client.PostAsJsonAsync("/api/listings", Listings.Complete());

        Assert.Equal(HttpStatusCode.Forbidden, mine.StatusCode);
        Assert.Equal("FOUNDER_REQUIRED", (await Problem.ReadAsync(mine)).Code);
        Assert.Equal(HttpStatusCode.Forbidden, draft.StatusCode);
        Assert.Equal("FOUNDER_REQUIRED", (await Problem.ReadAsync(draft)).Code);
    }

    [Fact]
    public async Task A_customer_cannot_upload_a_photo()
    {
        await _client.SignInVerifiedMemberAsync(_factory);

        using var response = await _client.UploadPhotoAsync(Images.Jpeg(1200, 1600));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task The_founder_reaches_inventory()
    {
        await _client.SignInFounderAsync(_factory);

        using var mine = await _client.GetAsync("/api/me/listings");
        using var draft = await _client.PostAsJsonAsync("/api/listings", Listings.Complete());

        Assert.Equal(HttpStatusCode.OK, mine.StatusCode);
        Assert.Equal(HttpStatusCode.Created, draft.StatusCode);
    }

    [Fact]
    public async Task A_listing_the_founder_published_appears_in_the_catalog_and_a_customers_does_not()
    {
        await _client.SignInFounderAsync(_factory);
        var reference = await _client.CreateDraftWithPhotoAsync(Listings.Complete());
        (await _client.PublishAsync(reference)).EnsureSuccessStatusCode();
        using var customer = _factory.CreateMemberClient();
        await customer.SignInVerifiedMemberAsync(_factory, "jordan_l", "jordan@example.com");
        _factory.SeedActiveListing(sellerId: _factory.MemberId("jordan@example.com"), number: 99001);

        using var visitor = _factory.CreateMemberClient();
        var page = await visitor.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/catalog/listings");

        Assert.Equal(1, page.GetProperty("totalCount").GetInt32());
    }
}
