namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a multi-image upload with a thumbnail carousel.</summary>
public sealed record ZoorceImageUploadOptions : ZoorceFileUploadOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateFileRenderOptions() with { Kind = ZoorceFormControlKind.ImageUpload };
}
