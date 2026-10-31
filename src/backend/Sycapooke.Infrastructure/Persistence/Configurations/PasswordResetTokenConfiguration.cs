using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sycapooke.Domain.Members;

namespace Sycapooke.Infrastructure.Persistence.Configurations;

internal sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("UX_PasswordResetTokens_TokenHash");
        builder.HasIndex(t => new { t.MemberId, t.CreatedAt }).HasDatabaseName("IX_PasswordResetTokens_MemberId_CreatedAt");
        builder.HasOne<Member>().WithMany().HasForeignKey(t => t.MemberId).OnDelete(DeleteBehavior.Cascade);
    }
}
