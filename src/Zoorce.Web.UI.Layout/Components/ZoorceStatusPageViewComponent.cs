using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Layout.Components;

/// <summary>Renders a standalone, host-configured status page without changing the HTTP response.</summary>
public sealed class ZoorceStatusPageViewComponent : ViewComponent
{
    /// <summary>Renders a status page.</summary>
    public IViewComponentResult Invoke(ZoorceStatusPageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        foreach (var action in options.Actions)
        {
            var url = action.Url?.Trim();
            if (string.IsNullOrWhiteSpace(url) || url.StartsWith("//", StringComparison.Ordinal) ||
                url.StartsWith("\\", StringComparison.Ordinal) || url.Any(char.IsControl) ||
                (Uri.TryCreate(url, UriKind.Absolute, out var absolute) && absolute.Scheme is not ("http" or "https")))
                throw new ArgumentException("Action destinations must be relative or HTTP(S) URLs.", nameof(options));
        }
        return View(options);
    }
}

/// <summary>Presentation options for a standalone status page. HTTP status and routing belong to the host.</summary>
public sealed class ZoorceStatusPageOptions
{
    /// <summary>Gets or sets the visible status code.</summary>
    public int StatusCode { get; set; }
    /// <summary>Gets or sets the application name.</summary>
    public string ApplicationName { get; set; } = "Zoorce Code";
    /// <summary>Gets or sets the short brand subtitle.</summary>
    public string? BrandSubtitle { get; set; } = "Powered by Elysium";
    /// <summary>Gets or sets the heading.</summary>
    public string Title { get; set; } = "Something went wrong.";
    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = "Please try again later.";
    /// <summary>Gets or sets optional host-approved diagnostic text. Never populated automatically.</summary>
    public string? Details { get; set; }
    /// <summary>Gets or sets the label for the optional diagnostic text.</summary>
    public string DetailsLabel { get; set; } = "Details";
    /// <summary>Gets or sets the astronaut's speech; null or empty hides the bubble.</summary>
    public string? Speech { get; set; } = "Houston, we have a problem.";
    /// <summary>Gets or sets whether to show an illustration.</summary>
    public bool ShowIllustration { get; set; } = true;
    /// <summary>Gets or sets trusted host-provided markup to replace the astronaut.</summary>
    public IHtmlContent? IllustrationContent { get; set; }
    /// <summary>Gets or sets trusted host-provided content after the description.</summary>
    public IHtmlContent? AdditionalContent { get; set; }
    /// <summary>Gets or sets whether diagonal light trails are shown.</summary>
    public bool ShowShootingStars { get; set; } = true;
    /// <summary>Gets or sets an optional CSS class on the root.</summary>
    public string? CssClass { get; set; }
    /// <summary>Gets or sets host-owned action links; an empty list hides the actions.</summary>
    public IReadOnlyList<ZoorceStatusPageAction> Actions { get; set; } = [];

    /// <summary>Creates defaults for one of the five supported statuses; destinations are controlled by the host.</summary>
    public static ZoorceStatusPageOptions ForStatus(int statusCode, string homeUrl = "~/", string signInUrl = "~/sign-in", string? retryUrl = null)
    {
        var (title, description, action, destination) = statusCode switch
        {
            401 => ("Sign in to continue.", "Your session may have expired, or you need to sign in to access this page.", "Sign in", signInUrl),
            403 => ("Access denied.", "You don't have permission to access this page.", "Go to home", homeUrl),
            404 => ("Page not found.", "The page you're looking for may have moved or no longer exists.", "Go to home", homeUrl),
            500 => ("Something went wrong.", "An unexpected error interrupted your request. Please try again.", retryUrl is null ? "Go to home" : "Try again", retryUrl ?? homeUrl),
            503 => ("Service temporarily unavailable.", "We're unable to complete your request right now. Please try again later.", retryUrl is null ? "Go to home" : "Try again", retryUrl ?? homeUrl),
            _ => throw new ArgumentOutOfRangeException(nameof(statusCode), "Supported statuses are 401, 403, 404, 500 and 503.")
        };
        return new ZoorceStatusPageOptions { StatusCode = statusCode, Title = title, Description = description, Actions = [new(action, destination)] };
    }
}

/// <summary>A host-owned status page action.</summary>
public sealed record ZoorceStatusPageAction(string Text, string Url, bool Secondary = false);
