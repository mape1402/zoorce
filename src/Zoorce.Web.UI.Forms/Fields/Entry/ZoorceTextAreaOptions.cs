namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a multiline text input.</summary>
public sealed record ZoorceTextAreaOptions : ZoorceFieldOptions
{
    /// <summary>Gets the number of visible text rows.</summary>
    public int Rows { get; init; } = 4;

    /// <summary>Gets the maximum accepted character count.</summary>
    public int? MaximumLength { get; init; }

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.TextArea, Rows = Rows, MaximumLength = MaximumLength };
}
