namespace Zoorce.Web.UI.Forms;

/// <summary>Configures an emoji picker.</summary>
public sealed record ZoorceEmojiPickerOptions : ZoorceChoiceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.EmojiPicker };
}
