namespace Zoorce.Web.UI.Forms;

/// <summary>Describes an asset already persisted by the host and shown by a file control.</summary>
public sealed record ZoorceFileUploadItem(
    string Id,
    string Name,
    string? Url = null,
    string? ContentType = null,
    long? SizeBytes = null)
{
    /// <summary>Returns a safe, display-ready asset descriptor.</summary>
    public ZoorceFileUploadItem Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("An uploaded file requires an id.", nameof(Id));
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException("An uploaded file requires a name.", nameof(Name));
        }

        if (SizeBytes is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(SizeBytes), "File size cannot be negative.");
        }

        return this with
        {
            Id = Id.Trim(),
            Name = Name.Trim(),
            Url = string.IsNullOrWhiteSpace(Url) ? null : Url.Trim(),
            ContentType = string.IsNullOrWhiteSpace(ContentType) ? null : ContentType.Trim().ToLowerInvariant()
        };
    }
}
