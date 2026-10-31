namespace Sycapooke.Domain.Members;

/// <summary>
/// A registered person who can buy and sell once their email is verified (L2-001, L2-002, L2-003).
/// A member who joined with Google has no password (L2-088).
/// </summary>
public sealed class Member
{
    private Member()
    {
    }

    public Guid Id { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>Upper-cased display name, unique, so names compare ignoring case.</summary>
    public string NormalizedDisplayName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    /// <summary>Upper-cased email, unique, so addresses compare ignoring case.</summary>
    public string NormalizedEmail { get; private set; } = string.Empty;

    /// <summary>Null for a member who only signs in with a third-party account; a password sign-in then fails as a wrong password.</summary>
    public string? PasswordHash { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? EmailVerifiedAt { get; private set; }

    public bool IsEmailVerified => EmailVerifiedAt is not null;

    /// <summary>
    /// Holds the Administrator role of L2-010, which only resolves order problems (L2-036). It is
    /// granted in the database by operations; there is no screen for it (ADR orders/0007).
    /// </summary>
    public bool IsAdministrator { get; private set; }

    /// <summary>When the account was closed (L2-005); a closed member's profile is unavailable (L2-041).</summary>
    public DateTime? ClosedAt { get; private set; }

    /// <summary>The founder's public story, up to 500 characters; null until she writes it (ADR general/0010).</summary>
    public string? StoryBio { get; private set; }

    /// <summary>The founder's public city, optional; never a street address.</summary>
    public string? StoryCity { get; private set; }

    /// <summary>The founder's public state or region, optional.</summary>
    public string? StoryRegion { get; private set; }

    public const int StoryBioMaxLength = 500;

    public void UpdateStory(string? bio, string? city, string? region)
    {
        StoryBio = bio;
        StoryCity = city;
        StoryRegion = region;
    }

    public bool IsClosed => ClosedAt is not null;

    /// <summary>Failed password sign-ins counted since <see cref="FailedSignInsSince"/> (L2-007).</summary>
    public int FailedSignIns { get; private set; }

    public DateTime? FailedSignInsSince { get; private set; }

    /// <summary>While in the future, password sign-in is refused even with the right password (L2-007).</summary>
    public DateTime? LockedUntil { get; private set; }

    public static readonly TimeSpan LockoutWindow = TimeSpan.FromMinutes(15);

    public const int MaxFailedSignIns = 5;

    public bool IsLocked(DateTime now) => LockedUntil is { } until && until > now;

    /// <summary>Counts a failed sign-in inside the 15-minute window; returns true when this one locks the account.</summary>
    public bool RecordFailedSignIn(DateTime now)
    {
        if (FailedSignInsSince is not { } since || now - since > LockoutWindow)
        {
            FailedSignInsSince = now;
            FailedSignIns = 0;
        }

        FailedSignIns++;
        if (FailedSignIns < MaxFailedSignIns)
        {
            return false;
        }

        LockedUntil = now + LockoutWindow;
        FailedSignIns = 0;
        FailedSignInsSince = null;
        return true;
    }

    public void ClearSignInFailures()
    {
        FailedSignIns = 0;
        FailedSignInsSince = null;
        LockedUntil = null;
    }

    public static Member Register(string displayName, string email, string passwordHash, DateTime now) => new()
    {
        Id = Guid.NewGuid(),
        DisplayName = displayName,
        NormalizedDisplayName = Normalize(displayName),
        Email = email,
        NormalizedEmail = Normalize(email),
        PasswordHash = passwordHash,
        CreatedAt = now,
    };

    /// <summary>A member joining with an email a third party has verified: no password, verified at once (L2-088).</summary>
    public static Member JoinVerified(string displayName, string email, DateTime now) => new()
    {
        Id = Guid.NewGuid(),
        DisplayName = displayName,
        NormalizedDisplayName = Normalize(displayName),
        Email = email,
        NormalizedEmail = Normalize(email),
        CreatedAt = now,
        EmailVerifiedAt = now,
    };

    public static string Normalize(string value) => value.Trim().ToUpperInvariant();

    public void MarkEmailVerified(DateTime now)
    {
        EmailVerifiedAt ??= now;
    }

    /// <summary>Changes the private display name; the caller has validated the format and uniqueness.</summary>
    public void Rename(string displayName)
    {
        DisplayName = displayName;
        NormalizedDisplayName = Normalize(displayName);
    }

    /// <summary>Replaces the password hash; the caller has proved the right to and validated the new password.</summary>
    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void GrantAdministrator()
    {
        IsAdministrator = true;
    }

    public void Close(DateTime now)
    {
        ClosedAt ??= now;
    }

    /// <summary>When the personal data was erased after closure; null until then (L2-005).</summary>
    public DateTime? ErasedAt { get; private set; }

    /// <summary>The label orders show for an erased buyer.</summary>
    public const string FormerCustomer = "Former customer";

    /// <summary>Replaces the identity with unique placeholders and drops the password; the account is already closed.</summary>
    public void Erase(DateTime now)
    {
        if (ClosedAt is null || ErasedAt is not null)
        {
            return;
        }

        var token = Id.ToString("N");
        DisplayName = "closed-" + token[..12];
        NormalizedDisplayName = Normalize(DisplayName);
        Email = token + "@erased.invalid";
        NormalizedEmail = Normalize(Email);
        PasswordHash = null;
        StoryBio = null;
        StoryCity = null;
        StoryRegion = null;
        ErasedAt = now;
    }
}
