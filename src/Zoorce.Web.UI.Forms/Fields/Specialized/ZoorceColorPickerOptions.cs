namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a color picker with optional product-specific swatches.</summary>
public sealed record ZoorceColorPickerOptions : ZoorceChoiceFieldOptions
{
    /// <summary>Gets whether an alpha channel may be selected.</summary>
    public bool AllowAlpha { get; init; }

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.ColorPicker, AllowAlpha = AllowAlpha };
}
