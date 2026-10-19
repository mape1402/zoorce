namespace Zoerce.WebUI.Abstractions.Navigation;

/// <summary>
/// Represents a navigation entry rendered by a host application's web shell.
/// </summary>
public sealed record NavigationItem(
    string Text,
    string? Url = null,
    string? Icon = null,
    IReadOnlyList<NavigationItem>? Children = null,
    NavigationMatchMode MatchMode = NavigationMatchMode.Auto,
    IReadOnlyList<string>? ActiveUrls = null)
{
    /// <summary>
    /// Gets whether the item is a navigation group.
    /// </summary>
    public bool HasChildren => Children is { Count: > 0 };

    /// <summary>
    /// Gets the routes or fragments that should activate this item.
    /// Falls back to <see cref="Url"/> when none are specified.
    /// </summary>
    public IReadOnlyList<string> GetActiveUrls()
        => ActiveUrls is { Count: > 0 }
            ? ActiveUrls
            : string.IsNullOrWhiteSpace(Url) ? [] : [Url];
}
