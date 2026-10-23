namespace Zoorce.Web.UI.Abstractions.Notifications;

/// <summary>
/// Represents a notification rendered by the web shell.
/// </summary>
public sealed record ZoorceToastMessage(
    string Title,
    string Message,
    ZoorceToastKind Kind = ZoorceToastKind.Information,
    int DurationMilliseconds = 5200);
