using System.Globalization;

namespace Zoorce.Web.UI.Forms;

/// <summary>Configures an accessible, themeable time picker with native form submission.</summary>
public sealed record ZoorceTimePickerOptions
{
    /// <summary>Gets the unique DOM identifier for the picker.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the submitted field name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the visible field label.</summary>
    public string? Label { get; init; }

    /// <summary>Gets optional supporting guidance.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the initially selected time.</summary>
    public TimeOnly? Value { get; init; }

    /// <summary>Gets the first selectable time, inclusive.</summary>
    public TimeOnly? Minimum { get; init; }

    /// <summary>Gets the last selectable time, inclusive.</summary>
    public TimeOnly? Maximum { get; init; }

    /// <summary>Gets the retained quick-pick interval configuration.</summary>
    /// <remarks>The list picker always exposes every minute; this setting is retained for compatibility with existing hosts.</remarks>
    public int MinuteStep { get; init; } = 5;

    /// <summary>Gets the clock notation used for the visible value and chooser.</summary>
    public ZoorceTimePickerFormat Format { get; init; } = ZoorceTimePickerFormat.Locale;

    /// <summary>Gets the locale used when <see cref="Format"/> is <see cref="ZoorceTimePickerFormat.Locale"/>.</summary>
    public string? Locale { get; init; }

    /// <summary>Gets whether a time must be chosen before form submission.</summary>
    public bool Required { get; init; }

    /// <summary>Gets whether the picker is unavailable for interaction.</summary>
    public bool Disabled { get; init; }

    /// <summary>Gets whether the value is visible but immutable.</summary>
    public bool ReadOnly { get; init; }

    /// <summary>Validates and normalizes the time-picker configuration.</summary>
    public ZoorceTimePickerOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("A time picker requires an id.", nameof(Id));
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("A time picker requires a name.", nameof(Name));
        if (!Enum.IsDefined(Format)) throw new ArgumentOutOfRangeException(nameof(Format));
        if (MinuteStep is < 1 or > 30 || 60 % MinuteStep != 0) throw new ArgumentOutOfRangeException(nameof(MinuteStep), "The minute step must divide evenly into an hour.");
        if (Minimum is not null && Maximum is not null && Minimum > Maximum) throw new ArgumentException("Minimum cannot be later than maximum.", nameof(Minimum));
        if (Value is not null && (Minimum is not null && Value < Minimum || Maximum is not null && Value > Maximum)) throw new ArgumentException("The selected time must be within the configured bounds.", nameof(Value));
        return this with
        {
            Id = Id.Trim(),
            Name = Name.Trim(),
            Label = TrimOrNull(Label),
            Description = TrimOrNull(Description),
            Locale = NormalizeLocale(Locale)
        };
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeLocale(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try { return CultureInfo.GetCultureInfo(value.Trim()).Name; }
        catch (CultureNotFoundException) { throw new ArgumentException("The locale is not recognized.", nameof(value)); }
    }
}
