using Microsoft.EntityFrameworkCore;
using Sycapooke.Application.Shop;
using Sycapooke.Infrastructure.Persistence;

namespace Sycapooke.Migrator;

/// <summary>Applies the EF Core migrations of Sycapooke.Infrastructure to Sycapooke database.</summary>
public static class MigratorApp
{
    public const string ConnectionStringVariable = "ConnectionStrings__Sycapooke";

    public const string FounderVariable = "Shop__FounderMemberId";

    public const string ProvisionFounderFlag = "--provision-founder";

    /// <summary>
    /// Applies pending migrations when a connection string is given. Without one it lists the
    /// migrations in this build, connects to nothing and fails, so a misconfigured deploy stops.
    /// </summary>
    public static int Run(TextWriter output, string? connectionString)
    {
        var options = new DbContextOptionsBuilder<SycapookeDbContext>()
            .UseSqlServer(string.IsNullOrWhiteSpace(connectionString) ? "Server=unset" : connectionString)
            .Options;
        using var db = new SycapookeDbContext(options);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            output.WriteLine($"Set {ConnectionStringVariable} to apply these migrations:");
            foreach (var migration in db.Database.GetMigrations())
            {
                output.WriteLine($"  {migration}");
            }

            return 2;
        }

        ApplyMigrations(db, output);
        return 0;
    }

    /// <summary>Applies every pending migration. Sycapooke.Cli reset calls it too, so there is one migration path (ADR platform/0010).</summary>
    public static void ApplyMigrations(DbContext db, TextWriter output)
    {
        db.Database.Migrate();
        output.WriteLine("Migrations applied.");
    }

    /// <summary>
    /// The trusted operator's step that binds the one founder account (ADR platform/0018). Normal
    /// migration and deployment never call it. Exit 0: bound or already bound; 3: the configuration is
    /// missing or is not one member ID; 4: the account is unknown or unverified, or another founder is stored.
    /// </summary>
    public static int ProvisionFounder(TextWriter output, SycapookeDbContext db, string? configuredFounderId)
    {
        if (!Guid.TryParse(configuredFounderId, out var founderId) || founderId == Guid.Empty)
        {
            output.WriteLine($"Set {FounderVariable} to one member ID (a GUID).");
            return 3;
        }

        var outcome = new FounderProvisioner(db).ProvisionAsync(founderId, CancellationToken.None).GetAwaiter().GetResult();
        output.WriteLine(outcome switch
        {
            ProvisionOutcome.Bound => "Founder bound.",
            ProvisionOutcome.AlreadyBound => "Founder already bound; nothing changed.",
            ProvisionOutcome.ConflictsWithStoredBinding => "A different founder is already bound; refused.",
            ProvisionOutcome.UnknownMember => "No such account; refused.",
            _ => "The account must be open and have a verified email; refused.",
        });
        return outcome is ProvisionOutcome.Bound or ProvisionOutcome.AlreadyBound ? 0 : 4;
    }

    /// <summary>Opens the database and runs <see cref="ProvisionFounder(TextWriter, SycapookeDbContext, string?)"/>.</summary>
    public static int RunProvisionFounder(TextWriter output, string? connectionString, string? configuredFounderId)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            output.WriteLine($"Set {ConnectionStringVariable} to provision the founder.");
            return 2;
        }

        var options = new DbContextOptionsBuilder<SycapookeDbContext>().UseSqlServer(connectionString).Options;
        using var db = new SycapookeDbContext(options);
        return ProvisionFounder(output, db, configuredFounderId);
    }
}
