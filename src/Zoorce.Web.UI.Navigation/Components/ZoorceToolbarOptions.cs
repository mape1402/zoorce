using Zoorce.Web.UI.Foundation.Components;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Configures a responsive action toolbar.</summary>
public sealed record ZoorceToolbarOptions
{
    /// <summary>Gets the unique DOM identifier for the toolbar.</summary>
    public required string Id { get; init; }

    /// <summary>Gets an accessible toolbar label.</summary>
    public required string Label { get; init; }

    /// <summary>Gets action items rendered within the toolbar.</summary>
    public IReadOnlyList<ZoorceMenuItem> Items { get; init; } = [];
}
