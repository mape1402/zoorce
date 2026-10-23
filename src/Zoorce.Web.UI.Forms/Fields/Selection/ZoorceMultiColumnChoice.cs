namespace Zoorce.Web.UI.Forms;

/// <summary>Represents one row selectable through a multi-column combobox.</summary>
public sealed record ZoorceMultiColumnChoice
{
    /// <summary>Gets the stable submitted row value.</summary>
    public required string Value { get; init; }

    /// <summary>Gets values keyed by <see cref="ZoorceComboBoxColumn.Key"/>.</summary>
    public IReadOnlyDictionary<string, string?> Values { get; init; } = new Dictionary<string, string?>();

    /// <summary>Gets whether the row cannot be selected.</summary>
    public bool Disabled { get; init; }
}
