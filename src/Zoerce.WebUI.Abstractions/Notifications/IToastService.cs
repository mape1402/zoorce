namespace Zoerce.WebUI.Abstractions.Notifications;

/// <summary>
/// Collects toast notifications during the lifetime of an HTTP request.
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Adds a toast for the web shell to render.
    /// </summary>
    void Show(ZoorceToastMessage message);

    /// <summary>
    /// Returns pending messages and clears the current queue.
    /// </summary>
    IReadOnlyList<ZoorceToastMessage> Drain();
}
