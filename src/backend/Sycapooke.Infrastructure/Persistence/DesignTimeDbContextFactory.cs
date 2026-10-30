using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sycapooke.Infrastructure.Persistence;

/// <summary>Lets dotnet-ef build the model to add migrations. It never opens a connection.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SycapookeDbContext>
{
    public SycapookeDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<SycapookeDbContext>().UseSqlServer("Server=design-time;Database=Sycapooke").Options);
}
