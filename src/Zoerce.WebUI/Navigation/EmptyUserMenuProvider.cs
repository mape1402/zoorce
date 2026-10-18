using Zoerce.WebUI.Abstractions.UserContext;

namespace Zoerce.WebUI.Navigation;

internal sealed class EmptyUserMenuProvider : IUserMenuProvider
{
    public UserMenuProfile? GetCurrentUser() => null;
}
