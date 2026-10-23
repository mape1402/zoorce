using Zoorce.Web.UI.Abstractions.UserContext;

namespace Zoorce.Web.UI.Layout.UserContext;

/// <summary>Provides no user profile until a host supplies a user-context provider.</summary>
public sealed class EmptyUserMenuProvider : IUserMenuProvider
{
    /// <inheritdoc />
    public UserMenuProfile? GetCurrentUser() => null;
}
