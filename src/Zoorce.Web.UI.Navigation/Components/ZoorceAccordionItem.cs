using Microsoft.AspNetCore.Html;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Represents one independently expandable accordion section.</summary>
public sealed record ZoorceAccordionItem
{
    /// <summary>Gets the stable section identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the section heading.</summary>
    public required string Title { get; init; }

    /// <summary>Gets optional supporting heading text.</summary>
    public string? Description { get; init; }

    /// <summary>Gets body markup supplied by the host.</summary>
    public IHtmlContent? Content { get; init; }

    /// <summary>Gets whether the section begins expanded.</summary>
    public bool Expanded { get; init; }

    /// <summary>Gets whether the section cannot be toggled.</summary>
    public bool Disabled { get; init; }
}
