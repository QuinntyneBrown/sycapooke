using Sycapooke.Application.Abstractions;
using Sycapooke.Application.Common;

namespace Sycapooke.Application.Accounts;

/// <summary>
/// The password rules of L2-001, shared by registration, reset and change so they never drift:
/// 12 to 128 characters and not a known breached password.
/// </summary>
public static class PasswordRules
{
    /// <summary>Adds an error under <paramref name="field"/> when the password breaks a rule.</summary>
    public static void Validate(IBreachedPasswords breached, string password, string field, Dictionary<string, FieldError> errors)
    {
        if (password.Length < 12)
        {
            errors[field] = new("PASSWORD_TOO_SHORT", "Password must be at least 12 characters");
        }
        else if (password.Length > 128)
        {
            errors[field] = new("PASSWORD_TOO_LONG", "Password must be 128 characters or fewer");
        }
        else if (breached.Contains(password))
        {
            errors[field] = new("PASSWORD_BREACHED", "This password has appeared in a data breach. Choose another.");
        }
    }
}
