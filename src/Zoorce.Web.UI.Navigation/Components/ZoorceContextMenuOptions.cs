using Zoorce.Web.UI.Foundation.Components;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Configures a context menu attached to an existing host element.</summary>
public sealed record ZoorceContextMenuOptions
{
    /// <summary>Gets the unique DOM identifier for the context menu.</summary>
    public required string Id { get; init; }

    /// <summary>Gets a CSS selector identifying the element that opens this menu on context click.</summary>
    public required string TargetSelector { get; init; }

    /// <summary>Gets context actions in display order.</summary>
    public IReadOnlyList<ZoorceMenuItem> Items { get; init; } = [];
}
