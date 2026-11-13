namespace Zoorce.Web.UI.Theming;

/// <summary>Describes a built-in Zoorce theme preset for selectors, documentation and galleries.</summary>
/// <param name="Preset">The stable preset identifier used by configuration.</param>
/// <param name="DisplayName">The human-readable preset name.</param>
/// <param name="Description">A concise description of the preset's visual character.</param>
/// <param name="LightAccent">The representative accent used by the light palette.</param>
/// <param name="DarkAccent">The representative accent used by the dark palette.</param>
public sealed record ZoorceThemePresetInfo(
    ZoorceThemePreset Preset,
    string DisplayName,
    string Description,
    string LightAccent,
    string DarkAccent);
