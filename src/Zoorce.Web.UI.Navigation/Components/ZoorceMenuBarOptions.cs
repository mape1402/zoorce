using Zoorce.Web.UI.Foundation.Components;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Configures a horizontal application menu with nested entries.</summary>
public sealed record ZoorceMenuBarOptions
{
    /// <summary>Gets the unique DOM identifier for the menu bar.</summary>
    public required string Id { get; init; }

    /// <summary>Gets top-level menu entries.</summary>
    public IReadOnlyList<ZoorceMenuItem> Items { get; init; } = [];
}
