namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a file-upload control with optional managed upload behavior.</summary>
public record ZoorceFileUploadOptions : ZoorceFieldOptions
{
    /// <summary>Gets accepted MIME types or file extensions.</summary>
    public string? Accept { get; init; }

    /// <summary>Gets whether more than one file can be selected.</summary>
    public bool Multiple { get; init; }

    /// <summary>Gets the largest number of retained files.</summary>
    public int? MaximumFiles { get; init; }

    /// <summary>Gets the maximum size in bytes of an individual file.</summary>
    public long? MaximumFileSizeBytes { get; init; }

    /// <summary>Gets the maximum aggregate size in bytes.</summary>
    public long? MaximumTotalFileSizeBytes { get; init; }

    /// <summary>Gets persisted host assets shown before a new selection.</summary>
    public IReadOnlyList<ZoorceFileUploadItem> ExistingFiles { get; init; } = [];

    /// <summary>Gets the optional same-origin managed-upload endpoint.</summary>
    public string? UploadUrl { get; init; }

    /// <summary>Gets the moment selected files are sent to <see cref="UploadUrl"/>.</summary>
    public ZoorceFileUploadBehavior UploadBehavior { get; init; } = ZoorceFileUploadBehavior.Deferred;

    /// <summary>Gets the managed-upload action label.</summary>
    public string? UploadButtonText { get; init; }

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateFileRenderOptions() with { Kind = ZoorceFormControlKind.FileUpload };

    /// <summary>Creates the file-specific rendering configuration used by asset field variants.</summary>
    private protected ZoorceFormControlOptions CreateFileRenderOptions()
        => base.CreateRenderOptions() with
        {
            Accept = Accept,
            Multiple = Multiple,
            MaximumFiles = MaximumFiles,
            MaximumFileSizeBytes = MaximumFileSizeBytes,
            MaximumTotalFileSizeBytes = MaximumTotalFileSizeBytes,
            ExistingFiles = ExistingFiles,
            UploadUrl = UploadUrl,
            FileUploadBehavior = UploadBehavior,
            UploadButtonText = UploadButtonText
        };
}
