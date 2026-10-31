"""Cast and catalog shared by every mock. Today is Sun 25 Oct 2026.

Reuse these people, items and orders everywhere so a reviewer can follow one
story across screens. Never invent a new person or item in a screen module;
add it here first.
"""

TODAY = "Sun 25 Oct 2026"

# One founder is the only seller (ADR general/0010). Her display name, portrait and story are
# PLACEHOLDERS until she approves the real ones (see README open questions).
# Customers are private: a display name and an email, nothing public.
FOUNDER = "miss_ida"
PEOPLE = {
    "miss_ida": {"name": "Ida Campbell", "initials": "IC", "city": "Kingston, Jamaica", "since": "Jan 2026", "rating": None, "reviews": 0, "sales": 12, "email": "ida@sycapooke.example",
                 "bio": "Placeholder story: antiques and vintage treasures from my own collection, each one kept with care and sent on to a new home with its story."},
    "attic_treasures": {"name": "Tessa Bell", "initials": "TB", "email": "tessa.bell@example.com", "since": "Mar 2025"},
    "pia_collects": {"name": "Pia Reyes", "initials": "PR", "email": "pia.reyes@example.com", "since": "Aug 2026"},
}
ME = "attic_treasures"     # the signed-in customer in customer mocks

# Every item is vintage: at least 20 years old. The one accepted size is "Vintage" in
# every category (a vintage lock set by the server from the category).
# circa = the decade shown on cards and detail; tone = photo backdrop 1..6; img = assets/items/<img>.svg
ITEMS = {
    # The eight landing-page listings
    "garnet-brooch": {"id": "L-20931", "title": "Gold filigree brooch with garnets", "brand": "Coro", "circa": "c.1950s", "cat": "Jewelry", "sub": "Brooches and pins", "cond": "Used - Excellent", "price": 145, "ship": 6, "seller": "miss_ida", "img": "brooch", "tone": 6, "listed": "2 hours ago", "photos": 5},
    "camel-overcoat": {"id": "L-20790", "title": "Camel wool overcoat, size M", "brand": "Pendleton", "circa": "c.1970s", "cat": "Clothing", "sub": "Coats and jackets", "cond": "Used - Fair", "price": 120, "ship": 12, "seller": "miss_ida", "img": "overcoat", "tone": 5, "listed": "4 days ago", "photos": 3,
                       "flaws": "Small moth nibble on the inside hem, rewoven. One horn button replaced with a close match."},
    "tea-set": {"id": "L-20802", "title": "Rose & gilt bone china tea set", "brand": "Wedgwood", "circa": "c.1940s", "cat": "Housewares", "sub": "Tableware", "cond": "Used - Good", "price": 210, "ship": 14, "seller": "miss_ida", "img": "tea-set", "tone": 2, "listed": "4 days ago", "photos": 7, "status": "Reserved"},
    "transistor-radio": {"id": "L-20909", "title": "Walnut transistor radio, working", "brand": "Zenith", "circa": "c.1960s", "cat": "Electronics", "sub": "Radios", "cond": "Used - Good", "price": 95, "ship": 9, "seller": "miss_ida", "img": "radio", "tone": 2, "listed": "5 hours ago", "photos": 4},
    "pearl-necklace": {"id": "L-20915", "title": "Triple-strand pearl necklace", "brand": "Monet", "circa": "c.1960s", "cat": "Jewelry", "sub": "Necklaces", "cond": "Used - Excellent", "price": 260, "ship": 0, "seller": "miss_ida", "img": "pearls", "tone": 4, "listed": "3 hours ago", "photos": 5},
    "day-dress": {"id": "L-20877", "title": "Floral shirtwaist day dress", "brand": "Unlabelled", "circa": "c.1960s", "cat": "Clothing", "sub": "Dresses", "cond": "Used - Good", "price": 78, "ship": 9, "seller": "miss_ida", "img": "day-dress", "tone": 1, "listed": "Yesterday", "photos": 6},
    "ceramic-vase": {"id": "L-20855", "title": "Teal drip-glaze ceramic vase", "brand": "Royal Haeger", "circa": "c.1960s", "cat": "Housewares", "sub": "Ceramics and pottery", "cond": "Used - Good", "price": 64, "ship": 9, "seller": "miss_ida", "img": "vase", "tone": 6, "listed": "2 days ago", "photos": 3, "status": "Sold"},
    "film-camera": {"id": "L-20902", "title": "35mm film camera with leather case", "brand": "Pentax", "circa": "c.1970s", "cat": "Electronics", "sub": "Cameras", "cond": "Used - Excellent", "price": 180, "ship": 8, "seller": "miss_ida", "img": "film-camera", "tone": 3, "listed": "5 hours ago", "photos": 4},
    # More of the catalog
    "seiko-watch": {"id": "L-20811", "title": "Automatic wristwatch, steel bracelet", "brand": "Seiko", "circa": "c.1970s", "cat": "Jewelry", "sub": "Watches", "cond": "Used - Good", "price": 140, "ship": 6, "seller": "miss_ida", "img": "watch", "tone": 3, "listed": "3 days ago", "photos": 6},
    "bakelite-bangles": {"id": "L-20870", "title": "Bakelite bangle set, butterscotch and cherry", "brand": "Unsigned", "circa": "c.1940s", "cat": "Jewelry", "sub": "Bracelets", "cond": "Used - Excellent", "price": 85, "ship": 5, "seller": "miss_ida", "img": "bangles", "tone": 4, "listed": "Yesterday", "photos": 4},
    "cameo-earrings": {"id": "L-20925", "title": "Shell cameo clip earrings", "brand": "Trifari", "circa": "c.1950s", "cat": "Jewelry", "sub": "Earrings", "cond": "Used - Excellent", "price": 58, "ship": 5, "seller": "miss_ida", "img": "earrings", "tone": 6, "listed": "3 hours ago", "photos": 4},
    "opal-ring": {"id": "L-20918", "title": "Opal and sterling ring, size 7", "brand": "Unsigned", "circa": "c.1970s", "cat": "Jewelry", "sub": "Rings", "cond": "Used - Excellent", "price": 95, "ship": 5, "seller": "miss_ida", "img": "ring", "tone": 4, "listed": "4 hours ago", "photos": 3},
    "monet-chain": {"id": "L-20861", "title": "Gold-tone rope chain necklace", "brand": "Monet", "circa": "c.1980s", "cat": "Jewelry", "sub": "Necklaces", "cond": "New without tags", "price": 38, "ship": 5, "seller": "miss_ida", "img": "chain", "tone": 1, "listed": "Yesterday", "photos": 2},
    "wool-cardigan": {"id": "L-20840", "title": "Shadow-plaid wool cardigan, size L", "brand": "Pendleton", "circa": "c.1960s", "cat": "Clothing", "sub": "Knitwear", "cond": "Used - Good", "price": 72, "ship": 8, "seller": "miss_ida", "img": "cardigan", "tone": 5, "listed": "2 days ago", "photos": 6},
    "silk-blouse": {"id": "L-20921", "title": "Silk pussy-bow blouse, size S", "brand": "Unlabelled", "circa": "c.1970s", "cat": "Clothing", "sub": "Shirts and blouses", "cond": "Used - Good", "price": 42, "ship": 6, "seller": "miss_ida", "img": "blouse", "tone": 2, "listed": "4 hours ago", "photos": 2},
    "deadstock-jeans": {"id": "L-20912", "title": "501 jeans, deadstock, 32×32", "brand": "Levi's", "circa": "c.1990s", "cat": "Clothing", "sub": "Skirts and trousers", "cond": "New with tags", "price": 150, "ship": 8, "seller": "miss_ida", "img": "jeans", "tone": 3, "listed": "4 hours ago", "photos": 2},
    "felt-cloche": {"id": "L-20899", "title": "Wool felt cloche hat", "brand": "Unlabelled", "circa": "c.1920s", "cat": "Clothing", "sub": "Hats and accessories", "cond": "Used - Fair", "price": 65, "ship": 7, "seller": "miss_ida", "img": "cloche", "tone": 1, "listed": "6 hours ago", "photos": 3,
                    "flaws": "Grosgrain band faded on one side. Tiny moth hole at the back, hidden by the band."},
    "pyrex-bowls": {"id": "L-20866", "title": "Primary-colour mixing bowl set of four", "brand": "Pyrex", "circa": "c.1950s", "cat": "Housewares", "sub": "Glassware", "cond": "Used - Excellent", "price": 68, "ship": 10, "seller": "miss_ida", "img": "mixing-bowls", "tone": 1, "listed": "Yesterday", "photos": 4},
    "jasperware-plate": {"id": "L-20760", "title": "Blue jasperware cabinet plate", "brand": "Wedgwood", "circa": "c.1960s", "cat": "Housewares", "sub": "Decor", "cond": "Used - Excellent", "price": 45, "ship": 8, "seller": "miss_ida", "img": "plate", "tone": 6, "listed": "6 days ago", "photos": 3, "status": "Sold"},
    "linen-tablecloth": {"id": "L-20848", "title": "Embroidered linen tablecloth with 8 napkins", "brand": "Unlabelled", "circa": "c.1950s", "cat": "Housewares", "sub": "Linens", "cond": "Used - Good", "price": 48, "ship": 7, "seller": "miss_ida", "img": "tablecloth", "tone": 2, "listed": "2 days ago", "photos": 3},
    "desk-lamp": {"id": "L-20893", "title": "Brass gooseneck desk lamp, rewired", "brand": "Unsigned", "circa": "c.1950s", "cat": "Housewares", "sub": "Lighting", "cond": "Used - Good", "price": 55, "ship": 11, "seller": "miss_ida", "img": "desk-lamp", "tone": 3, "listed": "6 hours ago", "photos": 3},
    "fiesta-pitcher": {"id": "L-20876", "title": "Disc pitcher in turquoise", "brand": "Fiesta", "circa": "c.1950s", "cat": "Housewares", "sub": "Tableware", "cond": "Used - Excellent", "price": 75, "ship": 10, "seller": "miss_ida", "img": "pitcher", "tone": 5, "listed": "Yesterday", "photos": 3},
    "brass-candlesticks": {"id": "L-20744", "title": "Pair of brass candlesticks", "brand": "Unsigned", "circa": "c.1960s", "cat": "Housewares", "sub": "Decor", "cond": "Used - Good", "price": 30, "ship": 8, "seller": "miss_ida", "img": "candlesticks", "tone": 4, "listed": "Mon 12 Oct", "photos": 2, "status": "Removed"},
    "rotary-phone": {"id": "L-20922", "title": "Black rotary desk telephone", "brand": "Western Electric", "circa": "c.1950s", "cat": "Electronics", "sub": "Telephones", "cond": "Used - Good", "price": 110, "ship": 12, "seller": "miss_ida", "img": "telephone", "tone": 2, "listed": "4 hours ago", "photos": 2},
    "mantel-clock": {"id": "L-20898", "title": "Eight-day mantel clock, chiming", "brand": "Seth Thomas", "circa": "c.1930s", "cat": "Electronics", "sub": "Clocks", "cond": "Used - Good", "price": 140, "ship": 12, "seller": "miss_ida", "img": "mantel-clock", "tone": 3, "listed": "Tue 6 Oct", "photos": 2, "status": "Sold"},
    "typewriter": {"id": "L-20834", "title": "Portable typewriter with case", "brand": "Smith-Corona", "circa": "c.1960s", "cat": "Electronics", "sub": "Typewriters and office", "cond": "Used - Good", "price": 125, "ship": 18, "seller": "miss_ida", "img": "typewriter", "tone": 5, "listed": "2 days ago", "photos": 5},
    "turntable": {"id": "L-20882", "title": "Belt-drive turntable, working", "brand": "Sony", "circa": "c.1970s", "cat": "Electronics", "sub": "Audio", "cond": "Used - Good", "price": 140, "ship": 16, "seller": "miss_ida", "img": "turntable", "tone": 6, "listed": "Yesterday", "photos": 2},
    "daisy-brooch": {"id": "L-20940", "title": "Enamel daisy brooch", "brand": "Trifari", "circa": "c.1960s", "cat": "Jewelry", "sub": "Brooches and pins", "cond": "Used - Good", "price": 24, "ship": 5, "seller": "miss_ida", "img": "brooch", "tone": 6, "listed": "Today", "photos": 3, "status": "Draft"},
}

# Catalog order, newest first (L2-022). Only active and reserved items.
CATALOG = ["garnet-brooch", "cameo-earrings", "pearl-necklace", "opal-ring", "transistor-radio", "deadstock-jeans", "film-camera", "desk-lamp",
           "rotary-phone", "day-dress", "fiesta-pitcher", "pyrex-bowls", "turntable", "typewriter", "linen-tablecloth", "tea-set", "camel-overcoat"]

# (category, subcategories, the one accepted size). The size is stored and shown as "Vintage".
CATEGORIES = [
    ("Jewelry", ["Necklaces", "Brooches and pins", "Earrings", "Bracelets", "Rings", "Watches"], "Vintage"),
    ("Clothing", ["Dresses", "Coats and jackets", "Knitwear", "Shirts and blouses", "Skirts and trousers", "Hats and accessories"], "Vintage"),
    ("Housewares", ["Tableware", "Glassware", "Ceramics and pottery", "Lighting", "Linens", "Decor"], "Vintage"),
    ("Electronics", ["Radios", "Cameras", "Audio", "Telephones", "Clocks", "Typewriters and office"], "Vintage"),
]
CONDITIONS = ["New with tags", "New without tags", "Used - Excellent", "Used - Good", "Used - Fair"]
BRANDS = ["Trifari", "Coro", "Monet", "Pendleton", "Levi's", "Pyrex", "Fire-King", "Lenox", "Wedgwood", "Fiesta", "Zenith", "Kodak", "Polaroid", "Sony", "Seiko",
          "Smith-Corona", "Western Electric"]

# Orders. Totals: price + shipping + tax. Payment goes straight to the shop account (L2-030).
ORDERS = {
    # Tessa's purchases
    "SY-48213": {"item": "day-dress", "buyer": "attic_treasures", "seller": "miss_ida", "status": "Shipped", "date": "Thu 22 Oct", "tax": 7.72, "total": 94.72, "carrier": "USPS", "tracking": "9400 1118 9922 3456 7890 12"},
    "SY-47790": {"item": "mantel-clock", "buyer": "attic_treasures", "seller": "miss_ida", "status": "Completed", "date": "Tue 13 Oct", "tax": 13.49, "total": 165.49},
    "SY-48140": {"item": "pyrex-bowls", "buyer": "attic_treasures", "seller": "miss_ida", "status": "Delivered", "date": "Mon 19 Oct", "tax": 6.92, "total": 84.92, "delivered": "Sat 24 Oct"},
    # Shop sales (founder only)
    "SY-48302": {"item": "ceramic-vase", "buyer": "pia_collects", "seller": "miss_ida", "status": "Paid", "date": "Sat 24 Oct", "tax": 6.48, "total": 79.48, "ship_by": "Fri 30 Oct",
                 "address": ["Pia Reyes", "1450 Larimer St, Apt 6B", "Denver, CO 80202"]},
    "SY-47511": {"item": "jasperware-plate", "buyer": "pia_collects", "seller": "miss_ida", "status": "Completed", "date": "Fri 2 Oct", "tax": 4.70, "total": 57.70},
}

SAVED_ADDRESS = ["Tessa Bell", "214 Kent Ave, Apt 3R", "Brooklyn, NY 11249"]
CARD = ("Visa", "4242")
