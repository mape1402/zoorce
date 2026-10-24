using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Zoorce.Web.UI.Abstractions.UserContext;

namespace Zoorce.Web.UI.Layout.Components;

/// <summary>
/// Renders the reusable application top bar, including the optional user menu.
/// </summary>
public sealed class ZoorceTopBarViewComponent : ViewComponent
{
    private readonly ZoorceShellOptions _shell;
    private readonly IUserMenuProvider _userMenuProvider;

    /// <summary>Initializes the component with shell and user-context services.</summary>
    public ZoorceTopBarViewComponent(IOptions<ZoorceShellOptions> shell, IUserMenuProvider userMenuProvider)
    {
        _shell = shell?.Value ?? throw new ArgumentNullException(nameof(shell));
        _userMenuProvider = userMenuProvider ?? throw new ArgumentNullException(nameof(userMenuProvider));
    }

    /// <summary>Renders the top bar for a page title.</summary>
    public IViewComponentResult Invoke(string? title = null)
    {
        var profile = _userMenuProvider.GetCurrentUser();
        var initials = string.IsNullOrWhiteSpace(profile?.DisplayName)
            ? _shell.BrandMark
            : profile!.DisplayName[..1].ToUpperInvariant();

        return View(new ZoorceTopBarModel(
            string.IsNullOrWhiteSpace(title) ? _shell.ApplicationName : title.Trim(),
            _shell.ApplicationName,
            profile,
            initials,
            _shell.DefaultTheme == Theming.ZoorceThemeMode.Dark,
            _shell.NotificationCenter));
    }
}

/// <summary>Supplies presentation data to <see cref="ZoorceTopBarViewComponent"/>.</summary>
public sealed record ZoorceTopBarModel(
    string Title,
    string ApplicationName,
    UserMenuProfile? Profile,
    string Initials,
    bool StartsDark,
    Zoorce.Web.UI.Foundation.Components.ZoorceNotificationCenterOptions? NotificationCenter);
