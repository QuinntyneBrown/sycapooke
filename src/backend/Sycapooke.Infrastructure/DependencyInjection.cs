using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sycapooke.Application.Abstractions;
using Sycapooke.Application.Catalog;
using Sycapooke.Application.Listings;
using Sycapooke.Application.Notifications;
using Sycapooke.Application.Shop;
using Sycapooke.Infrastructure.Accounts;
using Sycapooke.Infrastructure.Carriers;
using Sycapooke.Infrastructure.Email;
using Sycapooke.Infrastructure.Payments;
using Sycapooke.Infrastructure.Persistence;
using Sycapooke.Infrastructure.Photos;
using Sycapooke.Infrastructure.Privacy;

namespace Sycapooke.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers Sycapooke database and the adapters behind the application ports.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SycapookeDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Sycapooke")));
        services.AddScoped<ISycapookeDbContext>(sp => sp.GetRequiredService<SycapookeDbContext>());
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<IBreachedPasswords, BundledBreachedPasswords>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();
        services.AddSingleton(ShopConfiguration.Read(configuration));
        FieldCipher.Use(FieldCipher.FromConfiguration(configuration["Privacy:FieldEncryptionKeys"]));
        services.Configure<AppOptions>(configuration.GetSection("App"));
        services.Configure<NotificationOptions>(configuration.GetSection("Notifications"));
        services.Configure<SellingOptions>(configuration.GetSection("Selling"));
        services.Configure<PhotoOptions>(configuration.GetSection("Photos"));
        services.Configure<CatalogOptions>(configuration.GetSection("Catalog"));
        services.Configure<PhotoStorageOptions>(configuration.GetSection("PhotoStorage"));
        services.AddSingleton<IImageInspector, SkiaImageInspector>();
        services.AddSingleton<IListingPhotoProcessor, SkiaPhotoProcessor>();
        services.AddSingleton<IPhotoStorage, FileSystemPhotoStorage>();
        services.AddSingleton<IPaymentProvider, UnconfiguredPaymentProvider>();
        services.AddSingleton<ICarrierTracking, UnconfiguredCarrierTracking>();
        return services;
    }
}

/// <summary>Reads <c>Shop:FounderMemberId</c>. A value that is set but not a single GUID stops startup; an unset value grants no access (ADR platform/0018).</summary>
internal static class ShopConfiguration
{
    public static ShopOptions Read(IConfiguration configuration)
    {
        var raw = configuration[$"{ShopOptions.Section}:{nameof(ShopOptions.FounderMemberId)}"];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new ShopOptions();
        }

        return Guid.TryParse(raw, out var id) && id != Guid.Empty
            ? new ShopOptions { FounderMemberId = id }
            : throw new InvalidOperationException("Shop:FounderMemberId must be one member ID (a GUID), not a list or another value");
    }
}
