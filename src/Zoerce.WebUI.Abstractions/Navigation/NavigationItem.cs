namespace Zoerce.WebUI.Abstractions.Navigation;

/// <summary>
/// Represents a navigation entry rendered by a host application's web shell.
/// </summary>
public sealed record NavigationItem(
    string Text,
    string? Url = null,
    string? Icon = null,
    IReadOnlyList<NavigationItem>? Children = null)
{
    /// <summary>
    /// Gets whether the item is a navigation group.
    /// </summary>
    public bool HasChildren => Children is { Count: > 0 };
}
