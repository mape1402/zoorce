namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a searchable dropdown whose options expose multiple columns.</summary>
public sealed record ZoorceMultiColumnComboboxOptions : ZoorceFieldOptions
{
    /// <summary>Gets column definitions in their display order.</summary>
    public IReadOnlyList<ZoorceComboBoxColumn> Columns { get; init; } = [new() { Key = "value", Header = "Value" }];

    /// <summary>Gets selectable option rows.</summary>
    public IReadOnlyList<ZoorceMultiColumnChoice> Items { get; init; } = [];

    /// <summary>Gets optional remote data source used for searching.</summary>
    public string? Source { get; init; }

    /// <summary>Gets the key used as the displayed selected value.</summary>
    public string? DisplayKey { get; init; }

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.MultiColumnCombobox };
}
