// Acceptance Test
// Traces to: L2-054
// Description: A shipping address is stored sealed in the database and still reads back whole to the founder.

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sycapooke.Api.Tests.Support;

namespace Sycapooke.Api.Tests;

public sealed class ShippingAddressEncryptionTests : IDisposable
{
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _buyer;

    public ShippingAddressEncryptionTests()
    {
        _buyer = _factory.CreateMemberClient();
    }

    [Fact]
    public async Task The_address_columns_hold_ciphertext_and_the_sale_page_shows_the_address()
    {
        await _buyer.SignInVerifiedMemberAsync(_factory);
        var (orderId, seller) = await _factory.SeedPaidOrderAsync(_buyer);

        var stored = new List<string>();
        _factory.InDatabase(db =>
        {
            using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT ShippingFullName, ShippingLine1, ShippingCity, ShippingState, ShippingZip, ShippingCountry FROM Orders";
            using var reader = command.ExecuteReader();
            Assert.True(reader.Read());
            for (var i = 0; i < reader.FieldCount; i++)
            {
                stored.Add(reader.GetString(i));
            }
        });
        var sale = await seller.GetFromJsonAsync<JsonElement>($"/api/sales/{orderId}");

        Assert.All(stored[..5], value => Assert.StartsWith("enc1:test1:", value, StringComparison.Ordinal));
        Assert.Equal("US", stored[5]);
        Assert.False(string.IsNullOrEmpty(sale.GetProperty("shipTo").GetProperty("line1").GetString()));
        Assert.DoesNotContain("enc1:", sale.GetProperty("shipTo").GetRawText(), StringComparison.Ordinal);
    }

    public void Dispose()
    {
        _buyer.Dispose();
        _factory.Dispose();
    }
}
