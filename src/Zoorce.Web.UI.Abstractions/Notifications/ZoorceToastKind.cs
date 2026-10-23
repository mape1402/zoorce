namespace Zoorce.Web.UI.Abstractions.Notifications;

/// <summary>
/// Defines the visual importance of a toast notification.
/// </summary>
public enum ZoorceToastKind
{
    /// <summary>A successful operation.</summary>
    Success,

    /// <summary>Neutral contextual information.</summary>
    Information,

    /// <summary>An action that needs attention.</summary>
    Warning,

    /// <summary>An unsuccessful operation.</summary>
    Danger
}
