using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sycapooke.Domain.Members;
using Sycapooke.Domain.Shop;

namespace Sycapooke.Infrastructure.Persistence.Configurations;

internal sealed class ShopIdentityConfiguration : IEntityTypeConfiguration<ShopIdentity>
{
    public void Configure(EntityTypeBuilder<ShopIdentity> builder)
    {
        builder.ToTable("ShopIdentity", t => t.HasCheckConstraint("CK_ShopIdentity_Singleton", "[Id] = 1"));
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.HasIndex(s => s.FounderMemberId).IsUnique().HasDatabaseName("UX_ShopIdentity_FounderMemberId");
        builder.HasOne<Member>().WithMany().HasForeignKey(s => s.FounderMemberId).OnDelete(DeleteBehavior.Restrict);
    }
}
