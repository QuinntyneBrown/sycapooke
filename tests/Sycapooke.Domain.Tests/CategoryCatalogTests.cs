// Unit Test
// Traces to: L2-011, L2-013
// Description: The category table is held in code and sets the only accepted size per category.

using Sycapooke.Domain.Listings;

namespace Sycapooke.Domain.Tests;

public sealed class CategoryCatalogTests
{
    [Fact]
    public void Catalog_holds_exactly_4_categories_and_24_subcategories()
    {
        Assert.Equal(["Jewelry", "Clothing", "Housewares", "Electronics"], CategoryCatalog.All.Select(c => c.Name));
        Assert.Equal(24, CategoryCatalog.All.Sum(c => c.Subcategories.Count));
        Assert.Equal(["Necklaces", "Brooches and pins", "Earrings", "Bracelets", "Rings", "Watches"], CategoryCatalog.Find("Jewelry")!.Subcategories);
        Assert.Equal(["Dresses", "Coats and jackets", "Knitwear", "Shirts and blouses", "Skirts and trousers", "Hats and accessories"], CategoryCatalog.Find("Clothing")!.Subcategories);
        Assert.Equal(["Tableware", "Glassware", "Ceramics and pottery", "Lighting", "Linens", "Decor"], CategoryCatalog.Find("Housewares")!.Subcategories);
        Assert.Equal(["Radios", "Cameras", "Audio", "Telephones", "Clocks", "Typewriters and office"], CategoryCatalog.Find("Electronics")!.Subcategories);
    }

    [Theory]
    [InlineData("Jewelry")]
    [InlineData("Clothing")]
    [InlineData("Housewares")]
    [InlineData("Electronics")]
    public void Every_category_accepts_only_vintage(string category)
    {
        Assert.Equal("Vintage", CategoryCatalog.AcceptedSizeFor(category));
        Assert.Equal("Vintage", CategoryCatalog.Find(category)!.SizeLabel);
    }

    [Theory]
    [InlineData("Accessories", null)]
    [InlineData("Dresses", null)]
    [InlineData("electronics", null)]
    [InlineData("Clothing", "Hoodies")]
    [InlineData("Electronics", "Necklaces")]
    [InlineData("Housewares", "Hats")]
    public void Unknown_category_or_subcategory_is_not_accepted(string category, string? subcategory)
    {
        Assert.False(CategoryCatalog.Accepts(category, subcategory));
    }

    [Theory]
    [InlineData("Jewelry", "Brooches and pins")]
    [InlineData("Housewares", "Ceramics and pottery")]
    [InlineData("Electronics", "Typewriters and office")]
    public void A_subcategory_of_its_own_category_is_accepted(string category, string subcategory)
    {
        Assert.True(CategoryCatalog.Accepts(category, subcategory));
    }
}
