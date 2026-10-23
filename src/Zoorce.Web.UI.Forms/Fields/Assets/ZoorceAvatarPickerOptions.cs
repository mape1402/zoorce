namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a single-image avatar picker.</summary>
public sealed record ZoorceAvatarPickerOptions : ZoorceFileUploadOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateFileRenderOptions() with { Kind = ZoorceFormControlKind.AvatarPicker, Multiple = false, MaximumFiles = 1 };
}
