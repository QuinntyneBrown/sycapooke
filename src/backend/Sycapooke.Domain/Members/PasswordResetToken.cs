namespace Sycapooke.Domain.Members;

/// <summary>A single-use password reset link, stored as a hash, valid for one hour (L2-009).</summary>
public sealed class PasswordResetToken
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);

    /// <summary>At most this many links are sent per address in <see cref="RequestWindow"/> (L2-009 AC5).</summary>
    public const int MaxRequestsPerWindow = 3;

    public static readonly TimeSpan RequestWindow = TimeSpan.FromHours(1);

    private PasswordResetToken()
    {
    }

    public Guid Id { get; private set; }

    public Guid MemberId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? UsedAt { get; private set; }

    public static PasswordResetToken Issue(Guid memberId, string tokenHash, DateTime now) => new()
    {
        Id = Guid.NewGuid(),
        MemberId = memberId,
        TokenHash = tokenHash,
        CreatedAt = now,
        ExpiresAt = now + Lifetime,
    };

    public bool IsUsable(DateTime now) => UsedAt is null && now <= ExpiresAt;

    public void MarkUsed(DateTime now) => UsedAt = now;
}
