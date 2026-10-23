using Zoorce.Web.UI.Abstractions.Notifications;

namespace Zoorce.Web.UI.Foundation.Notifications;

/// <summary>Stores scoped toast messages until the toast region renders them.</summary>
public sealed class ToastService : IToastService
{
    private readonly List<ZoorceToastMessage> messages = [];

    /// <inheritdoc />
    public void Show(ZoorceToastMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        messages.Add(message);
    }

    /// <inheritdoc />
    public IReadOnlyList<ZoorceToastMessage> Drain()
    {
        var pending = messages.ToArray();
        messages.Clear();
        return pending;
    }
}
