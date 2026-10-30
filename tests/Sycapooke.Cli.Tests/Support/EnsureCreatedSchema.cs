using Microsoft.EntityFrameworkCore;
using Sycapooke.Infrastructure.Persistence;

namespace Sycapooke.Cli.Tests.Support;

/// <summary>
/// Rebuilds the schema from the model. The migrations hold SQL Server SQL that SQLite cannot run,
/// so tests rebuild with EnsureCreated; the migration path is checked by hand against LocalDB.
/// </summary>
public sealed class EnsureCreatedSchema : IDatabaseSchema
{
    public void Rebuild(SycapookeDbContext db, TextWriter output)
    {
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        output.WriteLine("Schema created.");
    }
}
