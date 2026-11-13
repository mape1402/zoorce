using System.Text.Json;
using Zoorce.Web.UI.Theming;

namespace Zoorce.Web.UI.Layout.Components;

/// <summary>Configures the optional runtime preset selector rendered in the application top bar.</summary>
public sealed class ZoorceThemeSelectorOptions
{
    /// <summary>Gets or sets whether the selector is rendered. It is hidden by default.</summary>
    public bool Visible { get; set; }

    /// <summary>Gets or sets the accessible label displayed for the selector.</summary>
    public string Label { get; set; } = "Theme";

    /// <summary>Gets or sets the presets exposed by the selector. An empty collection exposes the complete built-in catalog.</summary>
    public IReadOnlyList<ZoorceThemePreset> Presets { get; set; } = [];

    /// <summary>Gets or sets the preset used when the visitor has no saved choice.</summary>
    public ZoorceThemePreset? DefaultPreset { get; set; }

    /// <summary>Gets or sets whether the visitor's selected preset is stored in local browser storage.</summary>
    public bool PersistSelection { get; set; } = true;

    /// <summary>Gets or sets the browser-storage key used when persistence is enabled.</summary>
    public string StorageKey { get; set; } = "zoorce-theme-preset";
}

/// <summary>Supplies the resolved preset selector data consumed by the top bar and runtime script.</summary>
public sealed class ZoorceThemeSelectorModel
{
    internal ZoorceThemeSelectorModel(string label, string storageKey, bool persistSelection, ZoorceThemePreset initialPreset, IReadOnlyList<ZoorceThemePresetInfo> presets)
    {
        Label = label;
        StorageKey = storageKey;
        PersistSelection = persistSelection;
        InitialPreset = initialPreset;
        Presets = presets;
        RuntimeCatalogJson = JsonSerializer.Serialize(presets.ToDictionary(
            preset => preset.Preset.ToString(),
            preset => new
            {
                light = ZoorceThemePresets.CreatePalette(preset.Preset, ZoorceThemeMode.Light).ToCssVariables(),
                dark = ZoorceThemePresets.CreatePalette(preset.Preset, ZoorceThemeMode.Dark).ToCssVariables(),
                lightAccent = preset.LightAccent,
                darkAccent = preset.DarkAccent
            }));
    }

    /// <summary>Gets the accessible label displayed by the selector.</summary>
    public string Label { get; }

    /// <summary>Gets the browser-storage key assigned to the selector.</summary>
    public string StorageKey { get; }

    /// <summary>Gets whether the current selection is persisted locally.</summary>
    public bool PersistSelection { get; }

    /// <summary>Gets the preset selected before a visitor's saved choice is applied.</summary>
    public ZoorceThemePreset InitialPreset { get; }

    /// <summary>Gets the presets available to the visitor.</summary>
    public IReadOnlyList<ZoorceThemePresetInfo> Presets { get; }

    /// <summary>Gets the trusted JSON catalog used by the local runtime selector.</summary>
    public string RuntimeCatalogJson { get; }

    internal static ZoorceThemeSelectorModel? Create(ZoorceShellOptions shell)
    {
        var options = shell.ThemeSelector;
        if (options?.Visible != true) return null;

        var allPresets = ZoorceThemePresets.Available;
        var allowed = options.Presets.Count == 0
            ? allPresets
            : allPresets.Where(preset => options.Presets.Contains(preset.Preset)).ToArray();

        if (allowed.Count == 0 || options.Presets.Any(preset => allPresets.All(available => available.Preset != preset)))
        {
            throw new ArgumentOutOfRangeException(nameof(options.Presets), "The theme selector must expose at least one supported preset.");
        }

        var initial = options.DefaultPreset ?? shell.Themes.ActivePreset ?? allowed[0].Preset;
        if (allowed.All(preset => preset.Preset != initial)) initial = allowed[0].Preset;

        var label = string.IsNullOrWhiteSpace(options.Label) ? "Theme" : options.Label.Trim();
        var storageKey = string.IsNullOrWhiteSpace(options.StorageKey) ? "zoorce-theme-preset" : options.StorageKey.Trim();
        return new ZoorceThemeSelectorModel(label, storageKey, options.PersistSelection, initial, allowed);
    }
}
