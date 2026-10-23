using Zoorce.Web.UI.Abstractions.Navigation;

namespace Zoorce.Web.UI.Layout.Navigation;

/// <summary>Provides an empty default navigation until a host supplies workspace entries.</summary>
public sealed class EmptyNavigationProvider : INavigationProvider
{
    /// <inheritdoc />
    public IReadOnlyList<NavigationItem> GetItems() => [];
}
