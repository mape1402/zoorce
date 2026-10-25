using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Layout.Components;

/// <summary>Renders an independent sign-in entry point. Authentication belongs to the host.</summary>
public sealed class ZoorceSignInViewComponent : ViewComponent
{
    /// <summary>Renders the sign-in presentation using a host-provided destination.</summary>
    public IViewComponentResult Invoke(ZoorceSignInOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.SignInEnabled) return View(options);
        if (string.IsNullOrWhiteSpace(options.SignInUrl))
            throw new ArgumentException("A sign-in destination is required.", nameof(options));
        var url = options.SignInUrl.Trim();
        if (url.StartsWith("//", StringComparison.Ordinal) || url.StartsWith("\\", StringComparison.Ordinal) ||
            (Uri.TryCreate(url, UriKind.Absolute, out var absolute) && absolute.Scheme is not ("http" or "https")) ||
            url.Any(char.IsControl))
            throw new ArgumentException("The sign-in destination must be a relative URL or an HTTP(S) URL.", nameof(options));
        return View(options);
    }
}

/// <summary>Configures the content and destination of the sign-in entry point.</summary>
public sealed class ZoorceSignInOptions
{
    /// <summary>Gets or sets whether a configured sign-in destination is available.</summary>
    public bool SignInEnabled { get; set; } = true;
    /// <summary>Gets or sets the host-owned sign-in or authentication challenge URL.</summary>
    public string SignInUrl { get; set; } = "~/";
    /// <summary>Gets or sets the application name.</summary>
    public string ApplicationName { get; set; } = "Zoorce";
    /// <summary>Gets or sets optional supporting text beneath the application name.</summary>
    public string? BrandSubtitle { get; set; }
    /// <summary>Gets or sets an optional wordmark image that replaces the text name and mark.</summary>
    public string? WordmarkUrl { get; set; }
    /// <summary>Gets or sets an optional CSS class for styling the wordmark image container.</summary>
    public string? WordmarkClass { get; set; }
    /// <summary>Gets or sets a short mark displayed when a logo URL is not supplied.</summary>
    public string BrandMark { get; set; } = "Z";
    /// <summary>Gets or sets an optional application logo URL.</summary>
    public string? LogoUrl { get; set; }
    /// <summary>Gets or sets an optional replacement illustration URL.</summary>
    public string? IllustrationUrl { get; set; }
    /// <summary>Gets or sets whether the hero displays animated diagonal light trails.</summary>
    public bool ShowShootingStars { get; set; } = true;
    /// <summary>Gets or sets the illustration's short introductory label.</summary>
    public string HeroLabel { get; set; } = "Connected by design";
    /// <summary>Gets or sets the illustration heading.</summary>
    public string HeroTitle { get; set; } = "Build connected experiences.";
    /// <summary>Gets or sets the illustration description.</summary>
    public string HeroDescription { get; set; } = "A unified foundation for the applications and services your teams build.";
    /// <summary>Gets or sets optional short highlights displayed beneath the hero description.</summary>
    public IReadOnlyList<string> HeroHighlights { get; set; } = [];
    /// <summary>Gets or sets the short label above the sign-in heading.</summary>
    public string Eyebrow { get; set; } = "Your workspace awaits";
    /// <summary>Gets or sets the sign-in heading.</summary>
    public string Title { get; set; } = "Welcome back.";
    /// <summary>Gets or sets the sign-in description.</summary>
    public string Description { get; set; } = "Sign in to continue to your workspace.";
    /// <summary>Gets or sets the sign-in button label.</summary>
    public string SignInText { get; set; } = "Sign in";
    /// <summary>Gets or sets optional supporting text below the button.</summary>
    public string? Hint { get; set; }
    /// <summary>Gets or sets optional footer links.</summary>
    public IReadOnlyList<ZoorceSignInLink> Links { get; set; } = [];
}

/// <summary>A footer link in the sign-in entry point.</summary>
public sealed record ZoorceSignInLink(string Text, string Url);
