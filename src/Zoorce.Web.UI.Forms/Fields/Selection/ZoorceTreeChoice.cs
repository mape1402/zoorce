namespace Zoorce.Web.UI.Forms;

/// <summary>Represents one hierarchical selection option.</summary>
public sealed record ZoorceTreeChoice
{
    /// <summary>Gets the submitted node value.</summary>
    public required string Value { get; init; }

    /// <summary>Gets the visible node label.</summary>
    public required string Label { get; init; }

    /// <summary>Gets an optional glyph displayed before the label.</summary>
    public string? Icon { get; init; }

    /// <summary>Gets whether the node cannot be selected.</summary>
    public bool Disabled { get; init; }

    /// <summary>Gets the node's child entries.</summary>
    public IReadOnlyList<ZoorceTreeChoice> Children { get; init; } = [];
}
