// Acceptance Test
// Traces to: L2-005
// Description: A customer closes their own account with their password. Open orders block it and are
// listed; the founder is refused; sessions end at once; the worker erases personal data and orders
// keep the buyer as "Former customer"; founder inventory is untouched.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Sycapooke.Api.Tests.Support;
using Sycapooke.Application.Accounts;
using Sycapooke.Domain.Listings;
using Sycapooke.Domain.Orders;

namespace Sycapooke.Api.Tests;

public sealed class CloseCustomerAccountTests : IDisposable
{
    private readonly SycapookeApiFactory _factory = new();
    private readonly HttpClient _customer;

    public CloseCustomerAccountTests()
    {
        _customer = _factory.CreateMemberClient();
    }

    [Fact]
    public async Task A_customer_with_no_open_orders_closes_the_account_and_is_signed_out()
    {
        await _customer.SignInVerifiedMemberAsync(_factory);

        using var response = await Close(Members.Password);
        using var me = await _customer.GetAsync("/api/auth/me");
        using var signIn = await _factory.CreateMemberClient().SignInAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, signIn.StatusCode);
    }

    [Fact]
    public async Task A_wrong_password_does_not_close_the_account()
    {
        await _customer.SignInVerifiedMemberAsync(_factory);

        using var response = await Close("not my password");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        using var me = await _customer.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task Open_orders_block_closure_and_only_the_customers_own_are_listed()
    {
        await _customer.SignInVerifiedMemberAsync(_factory);
        var (orderId, _) = await _factory.SeedPaidOrderAsync(_customer);

        using var response = await Close(Members.Password);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("ACCOUNT_HAS_OPEN_ORDERS", body.GetProperty("code").GetString());
        var blocking = Assert.Single(body.GetProperty("errors").EnumerateObject());
        Assert.Equal(orderId.ToString(), blocking.Name);
        Assert.Equal("Paid", blocking.Value.GetProperty("code").GetString());
    }

    [Fact]
    public async Task The_founder_cannot_close_the_account()
    {
        await _customer.SignInFounderAsync(_factory);

        using var response = await Close(Members.Password);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Erasure_removes_personal_data_keeps_the_order_and_shows_a_former_customer()
    {
        await _customer.SignInVerifiedMemberAsync(_factory);
        var (orderId, seller) = await _factory.SeedPaidOrderAsync(_customer);
        (await seller.AddTrackingAsync(orderId)).EnsureSuccessStatusCode();
        _factory.InDatabase(db =>
        {
            var order = db.Orders.Single(o => o.Id == orderId);
            db.Entry(order).Property(o => o.Status).CurrentValue = OrderStatus.Completed;
            db.SaveChanges();
        });
        (await Close(Members.Password)).EnsureSuccessStatusCode();

        var erased = await EraseAsync();
        var again = await EraseAsync();

        Assert.Equal(1, erased);
        Assert.Equal(0, again);
        _factory.InDatabase(db =>
        {
            var member = db.Members.Single(m => m.Email.EndsWith("@erased.invalid"));
            Assert.NotNull(member.ErasedAt);
            Assert.Null(member.PasswordHash);
            Assert.DoesNotContain(db.Members, m => m.Email == "marcus.bell@example.com");
            Assert.Single(db.Orders);
            Assert.All(db.Listings, l => Assert.NotEqual(ListingStatus.Removed, l.Status));
        });
        var sale = await seller.GetFromJsonAsync<JsonElement>($"/api/sales/{orderId}");
        Assert.Equal("Former customer", sale.GetProperty("buyerDisplayName").GetString());
    }

    private Task<HttpResponseMessage> Close(string password) =>
        _customer.PostAsJsonAsync("/api/account/closure", new { password });

    private async Task<int> EraseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(new EraseClosedCustomersCommand());
    }

    public void Dispose()
    {
        _customer.Dispose();
        _factory.Dispose();
    }
}
