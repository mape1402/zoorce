namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a password input.</summary>
public sealed record ZoorcePasswordInputOptions : ZoorceFieldOptions
{
    /// <summary>Gets the maximum accepted character count.</summary>
    public int? MaximumLength { get; init; }

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.Password, MaximumLength = MaximumLength };
}
