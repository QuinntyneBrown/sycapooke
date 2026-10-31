namespace Sycapooke.Cli;

/// <summary>One demo account. Every demo member signs in with <see cref="DemoCatalogue.Password"/>.</summary>
public sealed record DemoMember(string DisplayName, string Email);

/// <summary>
/// One demo listing as a seller would enter it. <see cref="Photos"/> are paths under the seed
/// photos folder, cover first. The size is never given: it comes from the category (L2-013).
/// </summary>
public sealed record DemoListing(
    string Category,
    string Subcategory,
    string Title,
    string Description,
    string Brand,
    string Condition,
    string? Flaws,
    decimal Price,
    decimal ShippingCost,
    IReadOnlyList<string> Photos);

/// <summary>
/// The fixed demo set (seed-demo-data design): the founder, 8 private customers and the founder's
/// 48 listings, 12 in each of the 4 categories. Every piece is at least 20 years old and says so in its description. Brands
/// come from Selling:SuggestedBrands; photos are the committed images in SeedPhotos/, two per
/// category, shared by that category's listings. A small demo, not a load or scale fixture
/// (ADR general/0002).
/// </summary>
public static class DemoCatalogue
{
    /// <summary>The shared demo password, documented in the CLI README. For local use only.</summary>
    public const string Password = "Sycapooke-demo-only-2026";

    /// <summary>The shop's founder and only seller. Her name and story are placeholders until she approves the real ones.</summary>
    public static readonly DemoMember Founder = new("MissIda", "ida@demo.sycapooke.test");

    /// <summary>The private customers: they shop and manage their own account, nothing else.</summary>
    public static readonly IReadOnlyList<DemoMember> Customers =
    [
        new("BroochBetty", "betty@demo.sycapooke.test"),
        new("TeacupLorraine", "lorraine@demo.sycapooke.test"),
        new("RadioWalter", "walter@demo.sycapooke.test"),
        new("OvercoatDennis", "dennis@demo.sycapooke.test"),
        new("PearlEvelyn", "evelyn@demo.sycapooke.test"),
        new("ShirtwaistJoan", "joan@demo.sycapooke.test"),
        new("GlazeCarol", "carol@demo.sycapooke.test"),
        new("FilmFrank", "frank@demo.sycapooke.test"),
    ];

    /// <summary>Every demo account, the founder first.</summary>
    public static readonly IReadOnlyList<DemoMember> Members = [Founder, .. Customers];

    private const string Brooch = "Jewelry/gold-filigree-brooch.jpg";
    private const string Pearls = "Jewelry/triple-strand-pearls.jpg";
    private const string Overcoat = "Clothing/camel-wool-overcoat.jpg";
    private const string Shirtwaist = "Clothing/floral-shirtwaist-dress.jpg";
    private const string TeaSet = "Housewares/bone-china-tea-set.jpg";
    private const string Vase = "Housewares/teal-drip-glaze-vase.jpg";
    private const string Radio = "Electronics/walnut-transistor-radio.jpg";
    private const string Camera = "Electronics/35mm-film-camera.jpg";

    private const string NewWithTags = "New with tags";
    private const string NewWithoutTags = "New without tags";
    private const string Excellent = "Used - Excellent";
    private const string Good = "Used - Good";
    private const string Fair = "Used - Fair";

    private const string JewelryCategory = "Jewelry";
    private const string ClothingCategory = "Clothing";
    private const string HousewaresCategory = "Housewares";
    private const string ElectronicsCategory = "Electronics";

    private static readonly IReadOnlyList<DemoListing> Jewelry =
    [
        new(JewelryCategory, "Brooches and pins", "Gold filigree brooch with garnets", "Gold-tone filigree brooch set with five deep red garnets, c. 1950s. The roll-over clasp is tight and every stone is secure. A piece that has been worn and loved, and is ready for its next lapel.", "Trifari", Excellent, null, 145m, 6m, [Brooch, Pearls]),
        new(JewelryCategory, "Necklaces", "Triple-strand pearl necklace", "Triple strand of graduated faux pearls with a rhinestone box clasp, c. 1960s. The strands are evenly knotted and the lustre is still soft and even. Sits just above the collarbone.", "Coro", Excellent, null, 260m, 6m, [Pearls, Brooch]),
        new(JewelryCategory, "Brooches and pins", "Leaf spray brooch, gold tone", "Gold-tone leaf spray brooch with a textured finish, c. 1960s. Signed on the back. Some softening of the plating on the high points that reads as a gentle patina.", "Monet", Good, null, 48m, 5m, [Brooch]),
        new(JewelryCategory, "Necklaces", "Opera-length faux pearl rope", "Single opera-length strand of faux pearls, c. 1970s, long enough to double. The pearls are evenly matched and the clasp works well, though the knots show some wear.", "Napier", Fair, "Some knots between the pearls have darkened and the clasp plating is worn.", 36m, 5m, [Pearls]),
        new(JewelryCategory, "Earrings", "Rhinestone cluster clip earrings", "Clear rhinestone cluster clip-on earrings, c. 1950s, with the kind of sparkle that fills a room. Clips are firm and every stone is present and bright.", "Weiss", Excellent, null, 85m, 5m, [Brooch, Pearls]),
        new(JewelryCategory, "Earrings", "Faux pearl drop screw-back earrings", "Faux pearl drop earrings with screw backs, c. 1960s. A simple, elegant pair with a hint of age to the pearl surface.", "Sarah Coventry", Good, null, 28m, 5m, [Pearls, Brooch]),
        new(JewelryCategory, "Bracelets", "Gold-tone link bracelet, boxed", "Gold-tone panel link bracelet, c. 1970s, new old stock in its original box. Never worn. The fold-over clasp closes with a satisfying click.", "Monet", NewWithoutTags, null, 72m, 5m, [Brooch]),
        new(JewelryCategory, "Bracelets", "Garnet-red rhinestone bracelet", "Garnet-red rhinestone line bracelet with a safety chain, c. 1950s. Stones are deep and lively; the setting has an honest patina.", "Eisenberg", Good, null, 110m, 5m, [Brooch, Pearls]),
        new(JewelryCategory, "Rings", "Adjustable cocktail ring, faux pearl", "Faux pearl cocktail ring on an adjustable gold-tone band, c. 1960s. Signed. The pearl is unblemished and the band adjusts easily.", "Sarah Coventry", Excellent, null, 42m, 5m, [Pearls]),
        new(JewelryCategory, "Rings", "Rhinestone cluster dome ring", "Dome ring packed with clear rhinestones, c. 1970s. A statement piece with a couple of honest gaps.", "Weiss", Fair, "Two small rhinestones are missing from the lower edge of the dome.", 24m, 5m, [Brooch]),
        new(JewelryCategory, "Watches", "Gold-tone dress watch, manual wind", "Manual-wind gold-tone dress watch on its original bracelet, c. 1970s. Keeps good time after a recent service and the crystal is clear.", "Seiko", Excellent, null, 190m, 7m, [Brooch, Pearls]),
        new(JewelryCategory, "Watches", "Ladies' cocktail watch, c. 1960s", "Petite ladies' cocktail watch with a rhinestone bezel, c. 1960s. Runs, with the gentle wear you would expect from sixty years of evenings out.", "Seiko", Good, null, 98m, 7m, [Pearls]),
    ];

    private static readonly IReadOnlyList<DemoListing> Clothing =
    [
        new(ClothingCategory, "Coats and jackets", "Camel wool overcoat, size M", "Single-breasted camel wool overcoat, c. 1970s, with a notched lapel and a full satin lining. Fits a modern medium. Warm, structured and ready for another few winters.", "Pendleton", Excellent, null, 120m, 12m, [Overcoat, Shirtwaist]),
        new(ClothingCategory, "Dresses", "Floral shirtwaist day dress", "Cotton shirtwaist day dress in a small rose print, c. 1960s, with a self-fabric belt and a full skirt. Fits about a modern size 8. Pressed and ready to wear.", "Lilli Ann", Excellent, null, 78m, 8m, [Shirtwaist, Overcoat]),
        new(ClothingCategory, "Coats and jackets", "Plaid wool car coat", "Three-quarter length plaid wool car coat with a hood, c. 1960s. Heavy, warm and in wonderful shape apart from a little wear to the cuffs.", "Pendleton", Good, null, 95m, 12m, [Overcoat]),
        new(ClothingCategory, "Dresses", "Silk cocktail dress, midnight blue", "Midnight blue silk cocktail dress with a fitted bodice and a bow at the hip, c. 1950s. A glamorous piece with a small, honest repair under one arm.", "Christian Dior", Fair, "A neat mended seam under the left arm and light shattering at the hem lining.", 165m, 8m, [Shirtwaist]),
        new(ClothingCategory, "Knitwear", "Shawl-collar wool cardigan", "Oatmeal wool shawl-collar cardigan with leather buttons, c. 1970s. No holes, no moth, and still soft after decades of wear.", "Pendleton", Good, null, 58m, 8m, [Overcoat, Shirtwaist]),
        new(ClothingCategory, "Knitwear", "Striped wool pullover, deadstock", "Striped wool pullover, c. 1980s, deadstock with its original paper tag still attached. Never worn.", "Jantzen", NewWithTags, null, 64m, 8m, [Shirtwaist]),
        new(ClothingCategory, "Shirts and blouses", "Western snap shirt, pearl buttons", "Denim western shirt with pearl snaps and a yoke, c. 1970s. Softened by years of washing, with the fade earned honestly.", "Levi's", Good, null, 54m, 7m, [Overcoat]),
        new(ClothingCategory, "Shirts and blouses", "Silk pussy-bow blouse, ivory", "Ivory silk blouse with a pussy-bow neck and covered buttons, c. 1980s. Lovely drape; one faint mark near the hem that a dry clean may lift.", "Christian Dior", Fair, "A faint tea-coloured mark near the hem on the right side.", 46m, 7m, [Shirtwaist, Overcoat]),
        new(ClothingCategory, "Skirts and trousers", "Wool pencil skirt, charcoal", "Charcoal wool pencil skirt with a back kick pleat and a metal zip, c. 1960s. Fully lined and sharp as the day it was made.", "Lilli Ann", Excellent, null, 52m, 7m, [Shirtwaist]),
        new(ClothingCategory, "Skirts and trousers", "High-waist denim jeans, c. 1980s", "High-waist straight-leg jeans in a medium wash, c. 1980s, with the kind of fit people hunt for. Soft, worn in and whole.", "Levi's", Good, null, 88m, 8m, [Overcoat, Shirtwaist]),
        new(ClothingCategory, "Hats and accessories", "Wool felt cloche with grosgrain band", "Chocolate wool felt cloche with a grosgrain band and bow, c. 1960s. Holds its shape beautifully.", "Lilli Ann", Excellent, null, 45m, 7m, [Shirtwaist]),
        new(ClothingCategory, "Hats and accessories", "Printed silk scarf, boxed", "Large printed silk scarf in warm golds and browns, c. 1970s, still in its box. Never worn, with a very faint fold line.", "Jantzen", NewWithoutTags, null, 38m, 5m, [Overcoat]),
    ];

    private static readonly IReadOnlyList<DemoListing> Housewares =
    [
        new(HousewaresCategory, "Tableware", "Rose & gilt bone china tea set", "Bone china tea set for six, c. 1940s, with a pink rose pattern and gilt rims: teapot, creamer, sugar bowl, six cups and six saucers. No chips, no crazing, and the gilt is bright.", "Royal Doulton", Excellent, null, 210m, 18m, [TeaSet, Vase]),
        new(HousewaresCategory, "Ceramics and pottery", "Teal drip-glaze ceramic vase", "Mid-century teal drip-glaze ceramic vase, c. 1960s, about 10 inches tall. The glaze runs thick and glossy over a matte base. No chips or cracks.", "Dansk", Excellent, null, 64m, 10m, [Vase, TeaSet]),
        new(HousewaresCategory, "Tableware", "Set of six dinner plates, turquoise", "Six turquoise glazed dinner plates, c. 1950s, from a classic American pattern. Light utensil marks from decades of family dinners.", "Fiesta", Good, null, 96m, 16m, [TeaSet]),
        new(HousewaresCategory, "Tableware", "Blue and white serving platter", "Large blue and white transferware serving platter, c. 1960s. A fine hairline on the underside that does not show from the top.", "Wedgwood", Fair, "A fine hairline crack on the underside, about 2 inches, stable and invisible from above.", 42m, 14m, [TeaSet, Vase]),
        new(HousewaresCategory, "Glassware", "Jadeite coffee mugs, set of four", "Four milky green jadeite restaurant-ware mugs, c. 1950s. Heavy, glossy and chip-free.", "Fire-King", Excellent, null, 58m, 10m, [Vase]),
        new(HousewaresCategory, "Glassware", "Amber glass pitcher with ice lip", "Amber pressed-glass pitcher with an ice lip, c. 1970s. Holds about two quarts. Bright and clear with no chips.", "Corning", Good, null, 34m, 10m, [Vase, TeaSet]),
        new(HousewaresCategory, "Ceramics and pottery", "Nesting mixing bowls, primary colours", "Set of four nesting mixing bowls in red, yellow, green and blue, c. 1950s. Some fading to the red bowl from dishwasher years; still a working set.", "Pyrex", Fair, "The red bowl has faded and has a small flea-bite chip on the rim.", 72m, 14m, [TeaSet]),
        new(HousewaresCategory, "Ceramics and pottery", "Speckled stoneware teapot", "Speckled brown stoneware teapot with a bamboo-wrapped handle, c. 1970s. Pours cleanly and the lid fits snugly.", "Dansk", Good, null, 46m, 10m, [Vase]),
        new(HousewaresCategory, "Lighting", "Brass gooseneck desk lamp", "Brass gooseneck desk lamp with an enamelled shade, c. 1950s. Rewired with a modern cord and switch; the brass has a warm patina.", "Lenox", Good, null, 85m, 12m, [Vase, TeaSet]),
        new(HousewaresCategory, "Linens", "Embroidered linen tablecloth", "Hand-embroidered linen tablecloth with a drawn-thread border, c. 1940s, about 52 by 70 inches. Crisp, clean and whole.", "Wedgwood", Excellent, null, 54m, 8m, [TeaSet]),
        new(HousewaresCategory, "Decor", "Pair of porcelain figurines", "Pair of porcelain figurines, a lady and gentleman in period dress, c. 1960s. Both marked on the base. No chips, no repairs.", "Royal Doulton", Excellent, null, 120m, 12m, [TeaSet, Vase]),
        new(HousewaresCategory, "Decor", "Teak wall clock, boxed", "Teak-framed wall clock, c. 1970s, new old stock in its box. Battery movement tested and running.", "Lenox", NewWithoutTags, null, 68m, 12m, [Vase]),
    ];

    private static readonly IReadOnlyList<DemoListing> Electronics =
    [
        new(ElectronicsCategory, "Radios", "Walnut transistor radio, working", "Walnut-cased transistor radio, c. 1960s, with a brass-finished dial and a woven grille. Fully working on AM, with a warm tone and a clean volume pot.", "Zenith", Excellent, null, 95m, 10m, [Radio, Camera]),
        new(ElectronicsCategory, "Cameras", "35mm film camera with leather case", "35mm rangefinder film camera, c. 1970s, with its original leather ever-ready case. Shutter fires at all speeds and the meter responds. A camera that still wants to go out.", "Kodak", Excellent, null, 180m, 10m, [Camera, Radio]),
        new(ElectronicsCategory, "Radios", "Bakelite tabletop radio, c. 1950s", "Bakelite tabletop tube radio, c. 1950s. Cosmetically lovely; powers on and hums but needs a capacitor job to sing again. Sold for restoration.", "Philco", Fair, "Powers on but does not receive stations; needs recapping. One small chip on the back edge.", 60m, 14m, [Radio]),
        new(ElectronicsCategory, "Cameras", "Instant camera with flash, c. 1980s", "Folding instant camera with its flash bar, c. 1980s. Clean, tested with a fresh pack, ejects and develops as it should.", "Polaroid", Good, null, 75m, 8m, [Camera]),
        new(ElectronicsCategory, "Audio", "Personal cassette player, c. 1980s", "Compact personal cassette player, c. 1980s, with its belt clip. New belt fitted; plays and rewinds cleanly. Headphones not included.", "Sony", Good, null, 110m, 8m, [Radio, Camera]),
        new(ElectronicsCategory, "Audio", "Stereo receiver, brushed aluminium", "Brushed aluminium stereo receiver, c. 1970s, with a warm backlit dial. Both channels strong; one input selector is a little scratchy.", "Panasonic", Fair, "The aux input selector crackles when turned and the dial lamp is dim.", 140m, 20m, [Radio]),
        new(ElectronicsCategory, "Telephones", "Rotary desk telephone, black", "Classic black rotary desk telephone, c. 1960s. Dial spins true, the bell rings, and the cord is uncracked.", "Western Electric", Excellent, null, 88m, 12m, [Radio, Camera]),
        new(ElectronicsCategory, "Telephones", "Wall telephone in harvest gold", "Harvest gold wall telephone with a long coiled cord, c. 1970s. Works on a landline; the gold has aged to a soft amber.", "Western Electric", Good, null, 56m, 12m, [Camera]),
        new(ElectronicsCategory, "Clocks", "Flip clock radio, wood grain", "Wood-grain flip clock radio, c. 1970s. The numerals flip on the minute and the radio pulls in FM clearly.", "Panasonic", Good, null, 68m, 10m, [Radio]),
        new(ElectronicsCategory, "Clocks", "Electric kitchen clock, red", "Red electric kitchen wall clock with a sweep second hand, c. 1950s. Runs quietly and keeps time.", "RCA", Excellent, null, 48m, 10m, [Camera, Radio]),
        new(ElectronicsCategory, "Typewriters and office", "Portable manual typewriter with case", "Portable manual typewriter in seafoam green with its carry case, c. 1960s. Every key strikes, the bell rings, and a new ribbon is fitted.", "Smith-Corona", Excellent, null, 160m, 22m, [Camera, Radio]),
        new(ElectronicsCategory, "Typewriters and office", "Electric typewriter, boxed", "Electric typewriter, c. 1980s, new old stock in its original box with the manual. Powered up once to test.", "Smith-Corona", NewWithoutTags, null, 130m, 22m, [Radio]),
    ];

    /// <summary>
    /// Every listing in publish order, oldest first. The categories alternate, so the newest
    /// listings on the home page mix all 4 categories, and listings that share a cover photo are
    /// spread out instead of side by side.
    /// </summary>
    public static readonly IReadOnlyList<DemoListing> Listings =
        Interleave(Spread(Jewelry), Spread(Clothing), Spread(Housewares), Spread(Electronics));

    /// <summary>Every photo file the catalogue uses, relative to the seed photos folder.</summary>
    public static IReadOnlyList<string> PhotoFiles { get; } = Listings.SelectMany(l => l.Photos).Distinct().Order(StringComparer.Ordinal).ToList();

    /// <summary>The even entries, then the odd ones.</summary>
    private static List<DemoListing> Spread(IReadOnlyList<DemoListing> listings) =>
        listings.Where((_, i) => i % 2 == 0).Concat(listings.Where((_, i) => i % 2 == 1)).ToList();

    private static List<DemoListing> Interleave(params List<DemoListing>[] categories) =>
        Enumerable.Range(0, categories[0].Count).SelectMany(i => categories.Select(c => c[i])).ToList();
}
