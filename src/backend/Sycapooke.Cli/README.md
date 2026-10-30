# Sycapooke.Cli

A developer tool that gives a local Sycapooke install that looks real for a demo. It is for a
developer's own machine and local use only: deployments run `Sycapooke.Migrator`, never this
tool, and CI never runs it against a database
([ADR platform/0010](../../../docs/adr/platform/0010-sycapooke-cli-for-demo-seeding.md),
[design](../../../docs/detailed-designs/platform/seed-demo-data/README.md)).

## Commands

| Command | What it does |
|---------|--------------|
| `reset [--force]` | Drops the database, applies every EF Core migration (the same code as `Sycapooke.Migrator`) and empties the photo store. |
| `seed [--reset] [--force]` | Loads 8 demo members and 48 Active listings, 12 in each of Jewelry, Clothing, Housewares and Electronics, every one at least 20 years old, each with 1 to 2 photos from its category's folder. `--reset` runs `reset` first. |
| `help` | Prints usage. |

`reset` (and `seed --reset`) only runs when `DOTNET_ENVIRONMENT` is `Development`, unless
`--force` is passed. It prints the target server and database first. When the environment
is not set it counts as `Production`, so the guard is on by default.

`seed` goes through the same application commands as the website: members register and
verify their email, photos go through the upload pipeline (metadata stripped, WebP and JPEG
versions), listings are created as drafts and then published. Running `seed` again when
the demo is already there changes nothing; `seed --reset` always gives the same fresh demo.

## Settings

| Setting | Notes |
|---------|-------|
| `ConnectionStrings__Sycapooke` | Required, the same setting as `Sycapooke.Migrator` and the API. |
| `DOTNET_ENVIRONMENT` | `Development` allows `reset`. |
| `PhotoStorage__RootPath` | Only if you changed it for the API; the default is the same folder the API uses. |

## Usage

From `src/backend`, with the database settings of the root README:

```bash
dotnet build Sycapooke.Cli
export ConnectionStrings__Sycapooke="Server=(localdb)\MSSQLLocalDB;Database=Sycapooke;Trusted_Connection=True;TrustServerCertificate=True"
export DOTNET_ENVIRONMENT=Development
dotnet Sycapooke.Cli/bin/Debug/net10.0/Sycapooke.Cli.dll seed --reset
```

On PowerShell use `$env:ConnectionStrings__Sycapooke = "..."` and
`$env:DOTNET_ENVIRONMENT = "Development"`. On Windows on Arm, LocalDB is x64 only: run the
built DLL with the x64 host, `& "C:\Program Files\dotnet\x64\dotnet.exe" Sycapooke.Cli\bin\Debug\net10.0\Sycapooke.Cli.dll seed --reset`
(the arm64 host fails with error 193).

## Exit codes

| Code | Meaning |
|------|---------|
| 0 | Success, help, or the demo data already exists |
| 1 | Unknown command or option |
| 2 | `ConnectionStrings__Sycapooke` is not set |
| 3 | `reset` refused: not Development and no `--force` |
| 4 | A seed photo is missing |
| 5 | The database cannot be reached, or a migration or seed step failed |

## Demo accounts

Every demo member uses the password **`Sycapooke-demo-only-2026`**. It is for local use only:
never seed a shared or production database.

| Display name | Email |
|--------------|-------|
| BroochBetty | `betty@demo.sycapooke.test` |
| TeacupLorraine | `lorraine@demo.sycapooke.test` |
| RadioWalter | `walter@demo.sycapooke.test` |
| OvercoatDennis | `dennis@demo.sycapooke.test` |
| PearlEvelyn | `evelyn@demo.sycapooke.test` |
| ShirtwaistJoan | `joan@demo.sycapooke.test` |
| GlazeCarol | `carol@demo.sycapooke.test` |
| FilmFrank | `frank@demo.sycapooke.test` |

Seller ratings show "No reviews yet": no orders, reviews, reports or data exports are seeded.

## Photos

The 8 images in [`SeedPhotos/`](SeedPhotos/) were AI-generated for this project from the
Sycapooke landing-page design and are covered by the repository's licence: 2 in each of
`Jewelry/`, `Clothing/`, `Housewares/` and `Electronics/`. A folder is named exactly as its
category, and every demo listing uses photos from its own category's folder, so listings in a
category share the same two images on purpose. Each file and what it shows is listed in
[`SeedPhotos/ATTRIBUTION.md`](SeedPhotos/ATTRIBUTION.md); keep that file with the images.
