// Acceptance Test
// Traces to: L2-003, L2-010
// Description: A trusted operator binds one existing, verified account as the founder. Repeating the
// same binding changes nothing, a second founder is refused, and bad configuration binds no one.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sycapooke.Domain.Members;
using Sycapooke.Infrastructure.Persistence;

namespace Sycapooke.Migrator.Tests;

public sealed class ProvisionFounderTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly SycapookeDbContext _db;
    private readonly StringWriter _output = new();

    public ProvisionFounderTests()
    {
        _connection.Open();
        _db = new SycapookeDbContext(new DbContextOptionsBuilder<SycapookeDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public void A_verified_account_is_bound_as_the_only_founder()
    {
        var founder = AddMember("ida", verified: true);

        var exit = MigratorApp.ProvisionFounder(_output, _db, founder.Id.ToString());

        Assert.Equal(0, exit);
        Assert.Equal(founder.Id, Assert.Single(_db.ShopIdentities.AsNoTracking().ToList()).FounderMemberId);
    }

    [Fact]
    public void Repeating_the_same_binding_is_a_no_op()
    {
        var founder = AddMember("ida", verified: true);
        MigratorApp.ProvisionFounder(_output, _db, founder.Id.ToString());

        var exit = MigratorApp.ProvisionFounder(_output, _db, founder.Id.ToString());

        Assert.Equal(0, exit);
        Assert.Single(_db.ShopIdentities.AsNoTracking().ToList());
    }

    [Fact]
    public void A_different_account_cannot_become_a_second_founder()
    {
        var first = AddMember("ida", verified: true);
        var second = AddMember("tessa", verified: true);
        MigratorApp.ProvisionFounder(_output, _db, first.Id.ToString());

        var exit = MigratorApp.ProvisionFounder(_output, _db, second.Id.ToString());

        Assert.Equal(4, exit);
        Assert.Equal(first.Id, Assert.Single(_db.ShopIdentities.AsNoTracking().ToList()).FounderMemberId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("3f2c6a52-8b9e-4d52-9d9a-0c1f5d1c7a11,7a1d3a52-8b9e-4d52-9d9a-0c1f5d1c7a22")]
    public void Missing_or_malformed_configuration_binds_no_one(string? configured)
    {
        AddMember("ida", verified: true);

        var exit = MigratorApp.ProvisionFounder(_output, _db, configured);

        Assert.Equal(3, exit);
        Assert.Empty(_db.ShopIdentities.AsNoTracking().ToList());
    }

    [Fact]
    public void An_unknown_or_unverified_account_is_refused()
    {
        var unverified = AddMember("ida", verified: false);

        Assert.Equal(4, MigratorApp.ProvisionFounder(_output, _db, Guid.NewGuid().ToString()));
        Assert.Equal(4, MigratorApp.ProvisionFounder(_output, _db, unverified.Id.ToString()));
        Assert.Empty(_db.ShopIdentities.AsNoTracking().ToList());
    }

    private Member AddMember(string name, bool verified)
    {
        var now = DateTime.UtcNow;
        var member = Member.Register(name, $"{name}@example.com", "hash", now);
        if (verified)
        {
            member.MarkEmailVerified(now);
        }

        _db.Members.Add(member);
        _db.SaveChanges();
        return member;
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
        _output.Dispose();
    }
}
