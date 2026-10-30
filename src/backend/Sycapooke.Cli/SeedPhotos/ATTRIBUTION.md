# Seed photos

Every image in this folder was AI-generated for this project from the Sycapooke landing-page
design (Magic Patterns, October 2026): a warm, softly lit product photograph of one vintage piece,
rendered as a 1024 × 1024 JPEG under 400 KB. There is one folder per category, named exactly as
the category (`Jewelry`, `Clothing`, `Housewares`, `Electronics`), and each demo listing uses
photos from its own category's folder. No stock photography or third-party image is used, so the
images are covered by the repository's licence like the rest of the code. See
[ADR platform/0010](../../../../docs/adr/platform/0010-sycapooke-cli-for-demo-seeding.md).

The seeder uploads them through the photo pipeline, which strips metadata and re-encodes them
again. Brand names on seeded listings come from `Selling:SuggestedBrands` and do not describe the
item in the image; no maker endorses or is depicted by any image. Listings in a category share
that category's two photos, so the same image appears on several demo listings on purpose.

| File | Shows |
|---|---|
| `Jewelry/gold-filigree-brooch.jpg` | Gold filigree brooch set with garnets, c. 1950s |
| `Jewelry/triple-strand-pearls.jpg` | Triple-strand pearl necklace with a decorative clasp, c. 1960s |
| `Clothing/camel-wool-overcoat.jpg` | Camel wool overcoat on a wooden hanger, c. 1970s |
| `Clothing/floral-shirtwaist-dress.jpg` | Floral shirtwaist day dress with a belted waist, c. 1960s |
| `Housewares/bone-china-tea-set.jpg` | Rose and gilt bone china tea set, c. 1940s |
| `Housewares/teal-drip-glaze-vase.jpg` | Teal drip-glaze ceramic vase, c. 1960s |
| `Electronics/walnut-transistor-radio.jpg` | Walnut-cased transistor radio, c. 1960s |
| `Electronics/35mm-film-camera.jpg` | 35mm film camera with its leather case, c. 1970s |
