namespace Zoorce.Web.UI.Forms;

/// <summary>Describes one visible column in a multi-column combobox.</summary>
public sealed record ZoorceComboBoxColumn
{
    /// <summary>Gets the key read from each option's values.</summary>
    public required string Key { get; init; }

    /// <summary>Gets the visible column title.</summary>
    public required string Header { get; init; }

    /// <summary>Gets whether the column aligns numeric values to the end.</summary>
    public bool Numeric { get; init; }
}
