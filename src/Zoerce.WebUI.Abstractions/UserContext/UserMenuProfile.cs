namespace Zoerce.WebUI.Abstractions.UserContext;

/// <summary>
/// Represents the optional signed-in user shown in the web-shell menu.
/// </summary>
public sealed record UserMenuProfile(
    string DisplayName,
    string? Email = null,
    string? AvatarUrl = null,
    IReadOnlyList<UserMenuAction>? Actions = null);

/// <summary>
/// Represents an action available from the user menu.
/// </summary>
public sealed record UserMenuAction(string Text, string Url, string? Icon = null);
