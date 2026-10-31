// Acceptance Test
// Traces to: L2-025, L2-026, L2-028
// Description: Anyone can open a listing by its reference and see its photos, details and seller
// summary; the viewer relation tells the page whether to offer Buy now; Draft and Removed
// listings are hidden from everyone but their seller with the same 404 as a missing listing.

using System.Net;
using System.Net.Http.Json;
using Sycapooke.Api.Tests.Support;
using Sycapooke.Domain.Listings;
using Sycapooke.Domain.Members;

namespace Sycapooke.Api.Tests;

public sealed class ListingDetailTests : IDisposable
{
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _client;
    private readonly DateTime _start = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _sellerId;
    private long _number = 20000;

    public ListingDetailTests()
    {
        _client = _factory.CreateMemberClient();
        var seller = Member.Register("marcus_12", "marcus.bell@example.com", "hash", _start);
        _factory.InDatabase(db =>
        {
            db.Members.Add(seller);
            db.SaveChanges();
        });
        _sellerId = seller.Id;
        _factory.BindFounder(seller.Id);
    }

    [Fact]
    public async Task A_visitor_sees_the_whole_listing_and_the_seller_summary()
    {
        var reference = Seed(photos: 3, shipping: 9m);

        using var response = await _client.GetAsync($"/api/catalog/listings/{reference}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("no-store", response.Headers.CacheControl!.ToString());
        var detail = (await response.Content.ReadFromJsonAsync<Detail>())!;
        Assert.Equal(reference, detail.Reference);
        Assert.Equal("Floral shirtwaist day dress", detail.Title);
        Assert.Equal("Pendleton", detail.Brand);
        Assert.Equal("Clothing", detail.Category);
        Assert.Equal("Dresses", detail.Subcategory);
        Assert.Equal("Vintage", detail.SizeLabel);
        Assert.Equal("Used - Good", detail.Condition);
        Assert.Null(detail.Flaws);
        Assert.Equal("Light wear, colours still bright, c. 1960s.", detail.Description);
        Assert.Equal("85.00", detail.Price);
        Assert.Equal("9.00", detail.ShippingCost);
        Assert.Equal("94.00", detail.Total);
        Assert.Equal("Active", detail.Status);
        Assert.Equal(3, detail.Photos.Count);
        Assert.Equal(["Photo 1 of Pendleton Floral shirtwaist day dress", "Photo 2 of Pendleton Floral shirtwaist day dress", "Photo 3 of Pendleton Floral shirtwaist day dress"], detail.Photos.Select(p => p.Alt));
        Assert.Equal("marcus_12", detail.Seller.DisplayName);
        Assert.Equal(0, detail.Seller.CompletedSalesCount);
        Assert.Equal("Anonymous", detail.Viewer.Relation);
        Assert.DoesNotContain("marcus.bell@example.com", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task A_used_fair_listing_shows_its_flaws()
    {
        var reference = Seed(condition: "Used - Fair", flaws: "Small scuff on the left toe box.");

        var detail = (await _client.GetFromJsonAsync<Detail>($"/api/catalog/listings/{reference}"))!;

        Assert.Equal("Used - Fair", detail.Condition);
        Assert.Equal("Small scuff on the left toe box.", detail.Flaws);
    }

    [Fact]
    public async Task The_founder_is_told_the_listing_is_theirs_and_a_customer_is_not()
    {
        var own = Seed();
        await _client.SignInVerifiedMemberAsync(_factory, "jordan_l", "jordan@example.com");
        var asCustomer = (await _client.GetFromJsonAsync<Detail>($"/api/catalog/listings/{own}"))!;
        using var founder = SignedInAsSeller();
        var asFounder = (await founder.GetFromJsonAsync<Detail>($"/api/catalog/listings/{own}"))!;

        Assert.Equal("Seller", asFounder.Viewer.Relation);
        Assert.Equal("OtherMember", asCustomer.Viewer.Relation);
    }

    [Fact]
    public async Task A_listing_of_anyone_but_the_founder_is_not_found()
    {
        var other = Seed(sellerId: SeedCustomer("jordan@example.com"));

        using var response = await _client.GetAsync($"/api/catalog/listings/{other}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(ListingStatus.Reserved)]
    [InlineData(ListingStatus.Sold)]
    public async Task Reserved_and_sold_listings_stay_open_with_their_status(ListingStatus status)
    {
        var reference = Seed(status: status);

        var detail = (await _client.GetFromJsonAsync<Detail>($"/api/catalog/listings/{reference}"))!;

        Assert.Equal(status.ToString(), detail.Status);
    }

    [Theory]
    [InlineData(ListingStatus.Draft)]
    [InlineData(ListingStatus.Removed)]
    public async Task Draft_and_removed_listings_are_404_like_a_missing_one(ListingStatus status)
    {
        var reference = Seed(status: status);

        using var hidden = await _client.GetAsync($"/api/catalog/listings/{reference}");
        using var missing = await _client.GetAsync("/api/catalog/listings/L-99999");

        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        var hiddenProblem = await Problem.ReadAsync(hidden);
        var missingProblem = await Problem.ReadAsync(missing);
        Assert.Equal("LISTING_NOT_FOUND", hiddenProblem.Code);
        Assert.Equal(missingProblem with { CorrelationId = null }, hiddenProblem with { CorrelationId = null });
    }

    [Fact]
    public async Task The_founder_can_open_their_own_draft()
    {
        using var founder = SignedInAsSeller();
        var reference = Seed(status: ListingStatus.Draft);

        var detail = (await founder.GetFromJsonAsync<Detail>($"/api/catalog/listings/{reference}"))!;

        Assert.Equal("Draft", detail.Status);
        Assert.Equal("Seller", detail.Viewer.Relation);
    }

    private HttpClient SignedInAsSeller()
    {
        Member seller = null!;
        _factory.InDatabase(db => seller = db.Members.Single(m => m.Id == _sellerId));
        return _factory.CreateMemberClient().WithSession(_factory, seller);
    }

    private Guid SeedCustomer(string email)
    {
        var customer = Member.Register("jordan_l", email, "hash", _start);
        _factory.InDatabase(db =>
        {
            db.Members.Add(customer);
            db.SaveChanges();
        });
        return customer.Id;
    }

    private Guid MemberIdOf(string email)
    {
        var id = Guid.Empty;
        _factory.InDatabase(db => id = db.Members.Single(m => m.NormalizedEmail == Member.Normalize(email)).Id);
        return id;
    }

    private string Seed(
        Guid? sellerId = null, ListingStatus status = ListingStatus.Active, int photos = 1, decimal shipping = 8m,
        string condition = "Used - Good", string? flaws = null)
    {
        var seller = sellerId ?? _sellerId;
        var details = new ListingDetails("Clothing", "Dresses", null, "Floral shirtwaist day dress", "Light wear, colours still bright, c. 1960s.", "Pendleton", condition, flaws, 85m, shipping);
        var listing = Listing.CreateDraft(seller, ++_number, details, _start);
        var created = Enumerable.Range(0, photos).Select(i =>
        {
            var hash = new string((char)('a' + i), 64);
            var versions = new[] { 400, 800, 1600 }.SelectMany(size => new[] { PhotoFormat.WebP, PhotoFormat.Jpeg }.Select(format =>
                new ListingPhotoVersion(size, format, Math.Min(size, 1200), Math.Min(size, 1200) * 4 / 3, ListingPhotoVersion.KeyFor(hash, size, format), 1000)));
            var photo = ListingPhoto.Create(Guid.NewGuid(), seller, "image/jpeg", 1000, hash, 1200, 1600, versions, _start);
            photo.AttachTo(listing.Id, i);
            return photo;
        }).ToList();
        if (status != ListingStatus.Draft)
        {
            Assert.Empty(listing.Publish(_start, photoCount: photos));
        }

        _factory.InDatabase(db =>
        {
            db.Listings.Add(listing);
            db.ListingPhotos.AddRange(created);
            if (status is not (ListingStatus.Draft or ListingStatus.Active))
            {
                // Reserving, selling and removing belong to later features; set the state directly.
                db.Entry(listing).Property(l => l.Status).CurrentValue = status;
            }

            db.SaveChanges();
        });
        return listing.Reference;
    }

    private sealed record Detail(
        Guid Id, string Reference, string Title, string Brand, string Category, string? Subcategory, string SizeLabel,
        string Condition, string? Flaws, string Description, string Price, string ShippingCost, string Total, string Status,
        DateTime PublishedAt, List<Photo> Photos, Seller Seller, Viewer Viewer);

    private sealed record Photo(int Width, int Height, string Alt, string Src, string Webp, string Jpeg);

    private sealed record Seller(string DisplayName, int CompletedSalesCount, decimal? RatingAverage, int PublishedReviewCount);

    private sealed record Viewer(string Relation);

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
