namespace Zoorce.Web.UI.Theming;

/// <summary>
/// Provides a thread-safe in-memory active theme service.
/// </summary>
public sealed class ZoorceThemeService : IZoorceThemeService
{
    private readonly object _sync = new();
    private ZoorceThemeDefinition _current;

    /// <summary>Initializes the service with the supplied default theme.</summary>
    public ZoorceThemeService(ZoorceThemeDefinition? initialTheme = null)
    {
        _current = (initialTheme ?? new ZoorceThemeDefinition()).Normalize();
    }

    /// <inheritdoc />
    public ZoorceThemeDefinition Current
    {
        get
        {
            lock (_sync)
            {
                return _current;
            }
        }
    }

    /// <inheritdoc />
    public event EventHandler<ZoorceThemeDefinition>? ThemeChanged;

    /// <inheritdoc />
    public void Apply(ZoorceThemeDefinition theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        var normalized = theme.Normalize();
        lock (_sync)
        {
            _current = normalized;
        }

        ThemeChanged?.Invoke(this, normalized);
    }
}
