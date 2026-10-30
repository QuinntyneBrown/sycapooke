namespace Sycapooke.Domain.Listings;

/// <summary>One accepted category, its subcategories and the single size Sycapooke accepts for it.</summary>
public sealed record CategoryDefinition(string Name, IReadOnlyList<string> Subcategories, string AcceptedSize, string SizeLabel);

/// <summary>
/// The L2-011 table. It is code, not configuration, because the vintage rule is the product's
/// premise: every piece on Sycapooke, from jewelry to electronics, is at least 20 years old, so
/// every category accepts only <see cref="Vintage"/> (ADR general/0006).
/// </summary>
public static class CategoryCatalog
{
    public const string Vintage = "Vintage";

    public static readonly IReadOnlyList<CategoryDefinition> All =
    [
        new("Jewelry", ["Necklaces", "Brooches and pins", "Earrings", "Bracelets", "Rings", "Watches"], Vintage, "Vintage"),
        new("Clothing", ["Dresses", "Coats and jackets", "Knitwear", "Shirts and blouses", "Skirts and trousers", "Hats and accessories"], Vintage, "Vintage"),
        new("Housewares", ["Tableware", "Glassware", "Ceramics and pottery", "Lighting", "Linens", "Decor"], Vintage, "Vintage"),
        new("Electronics", ["Radios", "Cameras", "Audio", "Telephones", "Clocks", "Typewriters and office"], Vintage, "Vintage"),
    ];

    public static CategoryDefinition? Find(string? category) =>
        All.FirstOrDefault(c => string.Equals(c.Name, category, StringComparison.Ordinal));

    /// <summary>True when the category is in the table and the subcategory, if given, belongs to it.</summary>
    public static bool Accepts(string? category, string? subcategory)
    {
        var definition = Find(category);
        return definition is not null && (subcategory is null || definition.Subcategories.Contains(subcategory));
    }

    public static string AcceptedSizeFor(string category) =>
        Find(category)?.AcceptedSize ?? throw new ArgumentException($"Category {category} is not accepted.", nameof(category));
}
