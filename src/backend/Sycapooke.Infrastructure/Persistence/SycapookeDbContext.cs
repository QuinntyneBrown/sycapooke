using Microsoft.EntityFrameworkCore;
using Sycapooke.Application.Abstractions;
using Sycapooke.Domain.Listings;
using Sycapooke.Domain.Members;
using Sycapooke.Domain.Notifications;
using Sycapooke.Domain.Orders;
using Sycapooke.Domain.Payouts;

namespace Sycapooke.Infrastructure.Persistence;

/// <summary>EF Core context over Sycapooke database (SQL Server). LINQ only, so all SQL is parameterised (L2-049).</summary>
public sealed class SycapookeDbContext(DbContextOptions<SycapookeDbContext> options) : DbContext(options), ISycapookeDbContext
{
    public DbSet<Member> Members => Set<Member>();

    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    public DbSet<PendingExternalJoin> PendingExternalJoins => Set<PendingExternalJoin>();

    public DbSet<Listing> Listings => Set<Listing>();

    public DbSet<ListingPhoto> ListingPhotos => Set<ListingPhoto>();

    public DbSet<Checkout> Checkouts => Set<Checkout>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Refund> Refunds => Set<Refund>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<SavedAddress> SavedAddresses => Set<SavedAddress>();

    public DbSet<PaymentWebhookEvent> PaymentWebhookEvents => Set<PaymentWebhookEvent>();

    public DbSet<Payout> Payouts => Set<Payout>();

    public DbSet<SellerPayoutAccount> SellerPayoutAccounts => Set<SellerPayoutAccount>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<OrderProblem> OrderProblems => Set<OrderProblem>();

    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SycapookeDbContext).Assembly);
}
