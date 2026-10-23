using Zoorce.Web.UI.Abstractions.UserContext;

namespace Zoorce.Sample.Infrastructure;

public sealed class SampleUserMenuProvider : IUserMenuProvider
{
    public UserMenuProfile GetCurrentUser() => new(
        "Mario Perez",
        "mario@zoorce.dev",
        Actions:
        [
            new("Profile settings", "/#forms", "◌"),
            new("Sign out", "/", "↗")
        ]);
}
