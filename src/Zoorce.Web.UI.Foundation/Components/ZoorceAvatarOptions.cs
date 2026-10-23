namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures a presentational user or entity avatar.</summary>
public sealed record ZoorceAvatarOptions
{
    /// <summary>Gets alternate text for the represented entity.</summary>
    public required string Label { get; init; }

    /// <summary>Gets an optional image URL.</summary>
    public string? ImageUrl { get; init; }

    /// <summary>Gets optional initials used when no image is available.</summary>
    public string? Initials { get; init; }

    /// <summary>Gets the avatar size in pixels.</summary>
    public int Size { get; init; } = 40;

    /// <summary>Gets optional presence or status tone.</summary>
    public string? Status { get; init; }
}
