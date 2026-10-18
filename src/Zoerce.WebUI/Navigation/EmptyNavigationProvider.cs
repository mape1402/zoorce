using Zoerce.WebUI.Abstractions.Navigation;

namespace Zoerce.WebUI.Navigation;

internal sealed class EmptyNavigationProvider : INavigationProvider
{
    public IReadOnlyList<NavigationItem> GetItems() => [];
}
