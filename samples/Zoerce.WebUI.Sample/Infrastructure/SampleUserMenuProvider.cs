using Zoerce.WebUI.Abstractions.UserContext;

namespace Zoerce.WebUI.Sample.Infrastructure;

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
