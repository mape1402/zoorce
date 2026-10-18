namespace Zoerce.WebUI.Abstractions.UserContext;

/// <summary>
/// Supplies optional user-menu data for the current request.
/// </summary>
public interface IUserMenuProvider
{
    /// <summary>
    /// Gets the current profile, or <see langword="null"/> when no user menu should be rendered.
    /// </summary>
    UserMenuProfile? GetCurrentUser();
}
