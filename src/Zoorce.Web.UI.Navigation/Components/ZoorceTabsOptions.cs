namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Configures a reusable tab list and associated content panels.</summary>
public sealed record ZoorceTabsOptions
{
    /// <summary>Gets the unique DOM identifier for the tab set.</summary>
    public required string Id { get; init; }

    /// <summary>Gets tabs in display order.</summary>
    public IReadOnlyList<ZoorceTabItem> Items { get; init; } = [];

    /// <summary>Gets whether inactive panels remain in the DOM after selection changes.</summary>
    public bool KeepPanelsMounted { get; init; } = true;
}
