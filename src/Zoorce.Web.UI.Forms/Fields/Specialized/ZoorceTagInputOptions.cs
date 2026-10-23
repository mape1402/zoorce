namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a tokenized tag-entry field.</summary>
public sealed record ZoorceTagInputOptions : ZoorceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.TagInput };
}
