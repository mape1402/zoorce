namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a mask-driven entry field.</summary>
public sealed record ZoorceMaskedInputOptions : ZoorceFieldOptions
{
    /// <summary>Gets the display mask using <c>#</c>, <c>A</c>, and <c>*</c> tokens.</summary>
    public string? Mask { get; init; }

    /// <summary>Gets the maximum accepted character count.</summary>
    public int? MaximumLength { get; init; }

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.Masked, Mask = Mask, MaximumLength = MaximumLength };
}
