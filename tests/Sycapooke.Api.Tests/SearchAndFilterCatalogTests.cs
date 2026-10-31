// Acceptance Test
// Traces to: L2-023, L2-024
// Description: A visitor narrows the catalog by keyword, category, subcategory, brand, condition
// and price, and sorts it; the panel's live count and the brand list come from the same rules.

using System.Net;
using System.Net.Http.Json;
using Sycapooke.Api.Tests.Support;
using Sycapooke.Domain.Listings;
using Sycapooke.Domain.Members;

namespace Sycapooke.Api.Tests;

public sealed class SearchAndFilterCatalogTests : IDisposable
{
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _client;
    private readonly DateTime _start = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _sellerId;
    private long _number = 10000;
    private int _minutes;

    public SearchAndFilterCatalogTests()
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
    public async Task A_keyword_matches_title_words_in_any_case()
    {
        var airMax = Seed("Floral shirtwaist day dress");
        Seed("Camel wool overcoat", brand: "Jantzen");

        Assert.Equal([airMax], await References("q=SHIRTWAIST%20dress"));
    }

    [Fact]
    public async Task A_keyword_matches_the_brand_ignoring_case()
    {
        var fleece = Seed("Shawl-collar cardigan", brand: "Lilli Ann");
        Seed("Floral shirtwaist day dress");

        Assert.Equal([fleece], await References("q=LILLI%20ANN"));
    }

    [Fact]
    public async Task Title_and_brand_matches_rank_above_description_matches()
    {
        var inTitle = Seed("Brass desk lamp", description: "Rewired, c. 1950s, works.");
        var inDescription = Seed("Linen tablecloth", description: "Sits beside a brass lamp, c. 1950s.");
        Seed("Camel wool overcoat", brand: "Jantzen");

        // inDescription is newer, so only the match tier puts inTitle first.
        Assert.Equal([inTitle, inDescription], await References("q=lamp"));
    }

    [Fact]
    public async Task A_keyword_longer_than_100_characters_is_truncated_before_searching()
    {
        var long96 = new string('z', 96);
        var airMax = Seed("Wool felt cloche", description: $"Code {long96} on the label.");
        var term = $"cloche {long96}q";

        Assert.Equal([airMax], await References($"q={Uri.EscapeDataString(term)}"));
    }

    [Fact]
    public async Task Category_and_condition_combine_with_and()
    {
        var match = Seed("Camel wool overcoat", condition: "New with tags");
        Seed("Floral shirtwaist day dress", condition: "Used - Good");
        Seed("Walnut transistor radio", category: "Electronics", subcategory: "Radios", condition: "New with tags");

        Assert.Equal([match], await References("category=clothing&condition=new-with-tags"));
    }

    [Fact]
    public async Task Subcategories_and_brands_each_combine_with_or()
    {
        var pendleton = Seed("Floral shirtwaist day dress", brand: "Pendleton");
        var jantzen = Seed("Camel wool overcoat", brand: "Jantzen");
        Seed("Shawl-collar cardigan", brand: "Lilli Ann");
        var brooch = Seed("Gold filigree brooch", brand: "Trifari", category: "Jewelry", subcategory: "Brooches and pins");
        Seed("Garnet rhinestone bracelet", brand: "Eisenberg", category: "Jewelry", subcategory: "Bracelets");

        Assert.Equal([jantzen, pendleton], await References("brand=Pendleton&brand=jantzen"));
        Assert.Equal([brooch], await References("category=jewelry&subcategory=brooches-and-pins"));
    }

    [Fact]
    public async Task Category_and_subcategory_slugs_match_names_with_spaces()
    {
        var brooch = Seed("Gold filigree brooch", category: "Jewelry", subcategory: "Brooches and pins");
        Seed("Triple-strand pearl necklace", category: "Jewelry", subcategory: "Necklaces");
        var camera = Seed("35mm film camera", category: "Electronics", subcategory: "Cameras");

        Assert.Equal([brooch], await References("category=jewelry&subcategory=brooches-and-pins"));
        Assert.Equal([camera], await References("category=electronics"));
        Assert.Equal([brooch], await References("category=Jewelry&subcategory=Brooches%20and%20pins"));
    }

    [Fact]
    public async Task A_price_range_is_inclusive_and_price_sorts_ascending_and_descending()
    {
        var twenty = Seed("Twenty", price: 20m);
        var fifty = Seed("Fifty", price: 50m);
        var middle = Seed("Middle", price: 35.5m);
        Seed("Too cheap", price: 19.99m);
        Seed("Too dear", price: 50.01m);

        Assert.Equal([twenty, middle, fifty], await References("min=20&max=50&sort=price-asc"));
        Assert.Equal([fifty, middle, twenty], await References("min=20&max=50&sort=price-desc"));
    }

    [Fact]
    public async Task A_minimum_above_the_maximum_is_refused()
    {
        using var response = await _client.GetAsync("/api/catalog/listings?min=200&max=50");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await Problem.ReadAsync(response);
        Assert.Equal("CATALOG_PRICE_RANGE_INVALID", problem.Code);
        Assert.Equal("Minimum price must be lower than maximum", problem.Title);
    }

    [Theory]
    [InlineData("category=hats")]
    [InlineData("category=clothing&subcategory=necklaces")]
    [InlineData("category=electronics&subcategory=sneakers")]
    [InlineData("category=tops")]
    [InlineData("subcategory=necklaces")]
    [InlineData("condition=mint")]
    [InlineData("sort=cheapest")]
    [InlineData("min=-1")]
    [InlineData("max=12.345")]
    [InlineData("min=ten")]
    public async Task An_unknown_or_malformed_filter_is_refused(string query)
    {
        using var response = await _client.GetAsync($"/api/catalog/listings?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("CATALOG_FILTER_INVALID", (await Problem.ReadAsync(response)).Code);
    }

    [Fact]
    public async Task The_count_applies_the_same_filters()
    {
        Seed("Camel wool overcoat", condition: "New with tags");
        Seed("Floral shirtwaist day dress", condition: "New with tags");
        Seed("Old pair", condition: "Used - Fair");
        Seed("Unpublished draft", condition: "New with tags", publish: false);

        var count = await _client.GetFromJsonAsync<CountDto>("/api/catalog/listings/count?condition=new-with-tags");

        Assert.Equal(2, count!.TotalCount);
        using var refused = await _client.GetAsync("/api/catalog/listings/count?min=9&max=1");
        Assert.Equal("CATALOG_PRICE_RANGE_INVALID", (await Problem.ReadAsync(refused)).Code);
    }

    [Fact]
    public async Task Brands_of_visible_listings_are_listed_by_count_then_name()
    {
        Seed("Floral shirtwaist day dress", brand: "Pendleton");
        Seed("Silk cocktail dress, midnight blue", brand: "Pendleton");
        Seed("Camel wool overcoat", brand: "Jantzen");
        Seed("Shawl-collar cardigan", brand: "Lilli Ann");
        Seed("Unpublished", brand: "Wedgwood", publish: false);

        var brands = await _client.GetFromJsonAsync<List<BrandDto>>("/api/catalog/brands");

        Assert.Equal([new BrandDto("Pendleton", 2), new BrandDto("Jantzen", 1), new BrandDto("Lilli Ann", 1)], brands);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<List<string>> References(string query)
    {
        using var response = await _client.GetAsync($"/api/catalog/listings?{query}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = (await response.Content.ReadFromJsonAsync<CatalogPage>())!;
        return page.Items.Select(i => i.Reference).ToList();
    }

    private string Seed(
        string title,
        string brand = "Pendleton",
        string description = "Light wear, colours still bright, c. 1960s.",
        string category = "Clothing",
        string subcategory = "Dresses",
        string condition = "Used - Good",
        decimal price = 85m,
        bool publish = true)
    {
        var now = _start.AddMinutes(++_minutes);
        var number = ++_number;
        var details = new ListingDetails(category, subcategory, null, title, description, brand, condition, condition == "Used - Fair" ? "Small mark on the hem." : null, price, 8m);
        var listing = Listing.CreateDraft(_sellerId, number, details, now);
        var hash = new string('a', 64);
        var versions = new[] { 400, 800, 1600 }.SelectMany(size => new[] { PhotoFormat.WebP, PhotoFormat.Jpeg }.Select(format =>
            new ListingPhotoVersion(size, format, Math.Min(size, 1200), Math.Min(size, 1200) * 4 / 3, ListingPhotoVersion.KeyFor(hash, size, format), 1000)));
        var photo = ListingPhoto.Create(Guid.NewGuid(), _sellerId, "image/jpeg", 1000, hash, 1200, 1600, versions, now);
        photo.AttachTo(listing.Id, 0);
        if (publish)
        {
            Assert.Empty(listing.Publish(now, photoCount: 1));
        }

        _factory.InDatabase(db =>
        {
            db.Listings.Add(listing);
            db.ListingPhotos.Add(photo);
            db.SaveChanges();
        });
        return listing.Reference;
    }

    private sealed record CountDto(int TotalCount);

    private sealed record BrandDto(string Name, int ListingCount);

    private sealed record CatalogPage(List<Card> Items, int Page, int PageSize, int TotalCount);

    private sealed record Card(string Reference, string Title);
}
