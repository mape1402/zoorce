using Zoorce.Web.UI.Foundation.Progress;

namespace Zoorce.Web.UI.Foundation.WaitDialog;

/// <summary>
/// Configures an explicit blocking wait dialog for long-running host operations.
/// </summary>
public sealed record ZoorceWaitDialogOptions
{
    /// <summary>Gets the unique DOM id of the dialog.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the dialog title.</summary>
    public string Title { get; init; } = "Working";

    /// <summary>Gets the explanatory message shown below the title.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the progress treatment shown inside the dialog.</summary>
    public ZoorceProgressOptions Progress { get; init; } = new() { Kind = ZoorceProgressKind.Circular };

    /// <summary>Gets whether the host can expose a cancellation action.</summary>
    public bool CanCancel { get; init; }

    /// <summary>Gets the cancellation action label.</summary>
    public string CancelLabel { get; init; } = "Cancel";

    /// <summary>Returns a normalized immutable wait-dialog configuration.</summary>
    public ZoorceWaitDialogOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("A wait dialog requires an id.", nameof(Id));
        }

        return this with
        {
            Id = Id.Trim(),
            Title = string.IsNullOrWhiteSpace(Title) ? "Working" : Title.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            Progress = (Progress ?? new ZoorceProgressOptions { Kind = ZoorceProgressKind.Circular }).Normalize(),
            CancelLabel = string.IsNullOrWhiteSpace(CancelLabel) ? "Cancel" : CancelLabel.Trim()
        };
    }
}
