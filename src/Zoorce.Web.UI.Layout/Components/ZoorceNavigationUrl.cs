namespace Zoorce.Web.UI.Layout.Components;

// Treat web-rooted paths as paths on every OS; System.Uri treats them as file URLs on Unix.
internal static class ZoorceNavigationUrl
{
    internal static bool IsSafe(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsControl)) return false;
        var url = value.Trim();
        if (url.Contains('\\') || url.StartsWith("//", StringComparison.Ordinal) ||
            url.StartsWith("~//", StringComparison.Ordinal)) return false;
        if (url.StartsWith('/') || url.StartsWith("~/", StringComparison.Ordinal)) return true;
        return !Uri.TryCreate(url, UriKind.Absolute, out var absolute) ||
            absolute.Scheme is "http" or "https";
    }
}
