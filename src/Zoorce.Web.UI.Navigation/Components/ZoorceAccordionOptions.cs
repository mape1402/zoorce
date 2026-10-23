namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Configures a reusable group of expandable content sections.</summary>
public sealed record ZoorceAccordionOptions
{
    /// <summary>Gets the unique DOM identifier for the accordion.</summary>
    public required string Id { get; init; }

    /// <summary>Gets accordion sections in display order.</summary>
    public IReadOnlyList<ZoorceAccordionItem> Items { get; init; } = [];

    /// <summary>Gets whether multiple sections may remain expanded.</summary>
    public bool AllowMultiple { get; init; }
}
