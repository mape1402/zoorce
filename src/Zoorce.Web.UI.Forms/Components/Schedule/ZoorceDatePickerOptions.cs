using System.Globalization;

namespace Zoorce.Web.UI.Forms;

/// <summary>Configures an accessible calendar date picker with native form submission.</summary>
public sealed record ZoorceDatePickerOptions
{
    /// <summary>Gets the unique DOM identifier for the picker.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the submitted field name. Range mode submits <c>-start</c> and <c>-end</c> fields.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the visible field label.</summary>
    public string? Label { get; init; }

    /// <summary>Gets optional supporting guidance.</summary>
    public string? Description { get; init; }

    /// <summary>Gets whether one date or a date range is selected.</summary>
    public ZoorceDatePickerMode Mode { get; init; } = ZoorceDatePickerMode.Single;

    /// <summary>Gets the initially selected date in single mode.</summary>
    public DateOnly? Value { get; init; }

    /// <summary>Gets the initially selected start date in range mode.</summary>
    public DateOnly? RangeStart { get; init; }

    /// <summary>Gets the initially selected end date in range mode.</summary>
    public DateOnly? RangeEnd { get; init; }

    /// <summary>Gets the first selectable date, inclusive.</summary>
    public DateOnly? Minimum { get; init; }

    /// <summary>Gets the last selectable date, inclusive.</summary>
    public DateOnly? Maximum { get; init; }

    /// <summary>Gets dates that cannot be selected.</summary>
    public IReadOnlyCollection<DateOnly> DisabledDates { get; init; } = [];

    /// <summary>Gets the locale used to format the visible value and month heading.</summary>
    public string? Locale { get; init; }

    /// <summary>Gets the first weekday shown by the calendar.</summary>
    public DayOfWeek FirstDayOfWeek { get; init; } = DayOfWeek.Monday;

    /// <summary>Gets whether a date must be chosen before form submission.</summary>
    public bool Required { get; init; }

    /// <summary>Gets whether the picker is unavailable for interaction.</summary>
    public bool Disabled { get; init; }

    /// <summary>Gets whether the value is visible but immutable.</summary>
    public bool ReadOnly { get; init; }

    /// <summary>Validates and normalizes the date picker configuration.</summary>
    public ZoorceDatePickerOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("A date picker requires an id.", nameof(Id));
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("A date picker requires a name.", nameof(Name));
        if (!Enum.IsDefined(Mode)) throw new ArgumentOutOfRangeException(nameof(Mode));
        if (!Enum.IsDefined(FirstDayOfWeek)) throw new ArgumentOutOfRangeException(nameof(FirstDayOfWeek));
        if (Minimum is not null && Maximum is not null && Minimum > Maximum) throw new ArgumentException("Minimum cannot be later than maximum.", nameof(Minimum));
        if (RangeStart is not null && RangeEnd is not null && RangeStart > RangeEnd) throw new ArgumentException("Range start cannot be later than range end.", nameof(RangeStart));
        var disabled = DisabledDates.Distinct().Order().ToArray();
        ValidateSelection(Value, disabled, nameof(Value));
        ValidateSelection(RangeStart, disabled, nameof(RangeStart));
        ValidateSelection(RangeEnd, disabled, nameof(RangeEnd));
        return this with
        {
            Id = Id.Trim(),
            Name = Name.Trim(),
            Label = TrimOrNull(Label),
            Description = TrimOrNull(Description),
            Locale = NormalizeLocale(Locale),
            DisabledDates = disabled
        };

        void ValidateSelection(DateOnly? date, IReadOnlyCollection<DateOnly> excluded, string parameter)
        {
            if (date is null) return;
            if (Minimum is not null && date < Minimum || Maximum is not null && date > Maximum) throw new ArgumentException("Selected dates must be within the configured bounds.", parameter);
            if (excluded.Contains(date.Value)) throw new ArgumentException("Selected dates cannot be disabled.", parameter);
        }
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeLocale(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try { return CultureInfo.GetCultureInfo(value.Trim()).Name; }
        catch (CultureNotFoundException) { throw new ArgumentException("The locale is not recognized.", nameof(value)); }
    }
}
