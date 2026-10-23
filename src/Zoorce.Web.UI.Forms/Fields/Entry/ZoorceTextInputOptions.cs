namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a single-line text input.</summary>
public sealed record ZoorceTextInputOptions : ZoorceFieldOptions
{
    /// <summary>Gets the maximum accepted character count.</summary>
    public int? MaximumLength { get; init; }

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.Text, MaximumLength = MaximumLength };
}
