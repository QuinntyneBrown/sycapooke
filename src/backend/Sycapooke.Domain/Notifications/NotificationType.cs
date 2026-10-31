namespace Sycapooke.Domain.Notifications;

/// <summary>
/// The emails Sycapooke sends (L2-042) for events that exist in the code. Each has a template and a deep
/// link; types for features not built yet are added
/// with those features. Marketing is the only type a member can turn off today (L2-043); nothing
/// queues it until marketing campaigns are built.
/// </summary>
public enum NotificationType
{
    EmailVerification,
    SaleMade,
    OrderShipped,
    OrderDelivered,
    ShipByReminder,
    OrderCancelled,
    OrderRefunded,
    CancellationRequested,
    ProblemReported,
    ProblemResolved,
    Marketing,
    DraftExpiryReminder,
    AccountLocked,
    PasswordReset,
    PasswordChanged,
    PurchaseConfirmation,
}
