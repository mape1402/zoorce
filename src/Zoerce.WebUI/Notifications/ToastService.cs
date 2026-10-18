using Zoerce.WebUI.Abstractions.Notifications;

namespace Zoerce.WebUI.Notifications;

internal sealed class ToastService : IToastService
{
    private readonly List<ZoorceToastMessage> messages = [];

    public void Show(ZoorceToastMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        messages.Add(message);
    }

    public IReadOnlyList<ZoorceToastMessage> Drain()
    {
        var pending = messages.ToArray();
        messages.Clear();
        return pending;
    }
}
