using System.Globalization;

namespace Zoerce.WebUI.Components.Forms;

/// <summary>
/// Configures one self-contained Zoorce form control. The contract intentionally stays transport-neutral:
/// standard form fields continue to submit without the JavaScript enhancement.
/// </summary>
public sealed record ZoorceFormControlOptions
{
    /// <summary>Gets the unique DOM identifier for the control.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the standard submitted field name.</summary>
    public string? Name { get; init; }

    /// <summary>Gets the visible label. Hidden controls do not render it.</summary>
    public string? Label { get; init; }

    /// <summary>Gets supporting guidance displayed beneath the label.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the control type.</summary>
    public ZoorceFormControlKind Kind { get; init; } = ZoorceFormControlKind.Text;

    /// <summary>Gets the primary value. Multiple-value controls use a comma-separated value or selected choices.</summary>
    public string? Value { get; init; }

    /// <summary>Gets an optional second value, used by range and date-range controls.</summary>
    public string? SecondaryValue { get; init; }

    /// <summary>Gets placeholder copy for direct-entry controls.</summary>
    public string? Placeholder { get; init; }

    /// <summary>Gets selectable choices.</summary>
    public IReadOnlyList<ZoorceFormChoice> Choices { get; init; } = [];

    /// <summary>Gets the lowest accepted numeric value.</summary>
    public decimal? Minimum { get; init; }

    /// <summary>Gets the highest accepted numeric value.</summary>
    public decimal? Maximum { get; init; }

    /// <summary>Gets the numeric increment used by numeric and range controls.</summary>
    public decimal? Step { get; init; }

    /// <summary>Gets a character limit for text-like values.</summary>
    public int? MaximumLength { get; init; }

    /// <summary>Gets the number of cells rendered by an <see cref="ZoorceFormControlKind.Otp"/> control.</summary>
    /// <remarks><see cref="MaximumLength"/> remains a backwards-compatible fallback for OTP controls.</remarks>
    public int? CodeLength { get; init; }

    /// <summary>Gets the characters accepted by an <see cref="ZoorceFormControlKind.Otp"/> control.</summary>
    public ZoorceOtpCharacterSet OtpCharacterSet { get; init; } = ZoorceOtpCharacterSet.Numeric;

    /// <summary>Gets the case normalization applied by an <see cref="ZoorceFormControlKind.Otp"/> control.</summary>
    public ZoorceOtpTextTransform OtpTextTransform { get; init; } = ZoorceOtpTextTransform.Uppercase;

    /// <summary>Gets the row count for text areas.</summary>
    public int Rows { get; init; } = 4;

    /// <summary>Gets a display mask. Use # for a digit, A for a letter and * for an alphanumeric character.</summary>
    public string? Mask { get; init; }

    /// <summary>Gets an optional leading adornment, such as a currency symbol.</summary>
    public string? Prefix { get; init; }

    /// <summary>Gets an optional trailing adornment, such as a unit.</summary>
    public string? Suffix { get; init; }

    /// <summary>Gets accepted MIME types or extensions for file inputs.</summary>
    public string? Accept { get; init; }

    /// <summary>Gets whether the file selector accepts multiple files.</summary>
    public bool Multiple { get; init; }

    /// <summary>Gets whether the user can create values not in <see cref="Choices"/>.</summary>
    public bool AllowCustomValues { get; init; }

    /// <summary>Gets whether color values can include an alpha channel.</summary>
    public bool AllowAlpha { get; init; }

    /// <summary>Gets whether a value must be supplied.</summary>
    public bool Required { get; init; }

    /// <summary>Gets whether the control is disabled.</summary>
    public bool Disabled { get; init; }

    /// <summary>Gets whether the value is visible but cannot be modified.</summary>
    public bool ReadOnly { get; init; }

    /// <summary>Gets validation state supplied by the host.</summary>
    public ZoorceFormControlState State { get; init; } = ZoorceFormControlState.Default;

    /// <summary>Gets validation copy associated with <see cref="State"/>.</summary>
    public string? ValidationMessage { get; init; }

    /// <summary>Gets the control's browser autocomplete token.</summary>
    public string? Autocomplete { get; init; }

    /// <summary>Gets an optional locale for currency and number formatting.</summary>
    public string? Locale { get; init; }

    /// <summary>Gets the ISO currency code used by currency controls.</summary>
    public string? CurrencyCode { get; init; }

    /// <summary>Gets optional host metadata. Templates and client code can use this without growing the base contract.</summary>
    public IReadOnlyDictionary<string, string?> Metadata { get; init; } = new Dictionary<string, string?>();

    /// <summary>Validates and normalizes the field configuration.</summary>
    public ZoorceFormControlOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("A form control requires an id.", nameof(Id));
        }

        if (!Enum.IsDefined(Kind))
        {
            throw new ArgumentOutOfRangeException(nameof(Kind), "The form control kind is not supported.");
        }

        if (!Enum.IsDefined(State))
        {
            throw new ArgumentOutOfRangeException(nameof(State), "The form control state is not supported.");
        }

        if (Minimum is not null && Maximum is not null && Minimum > Maximum)
        {
            throw new ArgumentException("The minimum cannot exceed the maximum.", nameof(Minimum));
        }

        if (Step is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Step), "A numeric step must be greater than zero.");
        }

        if (MaximumLength is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumLength), "Maximum length must be greater than zero.");
        }

        if (CodeLength is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(CodeLength), "Code length must be greater than zero.");
        }

        if (!Enum.IsDefined(OtpCharacterSet))
        {
            throw new ArgumentOutOfRangeException(nameof(OtpCharacterSet), "The OTP character set is not supported.");
        }

        if (!Enum.IsDefined(OtpTextTransform))
        {
            throw new ArgumentOutOfRangeException(nameof(OtpTextTransform), "The OTP text transform is not supported.");
        }

        if (Choices.Any(choice => choice is null || string.IsNullOrWhiteSpace(choice.Value)))
        {
            throw new ArgumentException("Choices require a value.", nameof(Choices));
        }

        var duplicateChoice = Choices
            .GroupBy(choice => choice.Value.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateChoice is not null)
        {
            throw new ArgumentException($"Choice '{duplicateChoice.Key}' is duplicated.", nameof(Choices));
        }

        var normalizedValue = NormalizeValue(Value);
        var normalizedCodeLength = Kind == ZoorceFormControlKind.Otp ? CodeLength ?? MaximumLength ?? 6 : CodeLength;
        if (Kind == ZoorceFormControlKind.Otp)
        {
            normalizedValue = NormalizeOtpValue(normalizedValue, normalizedCodeLength!.Value, OtpCharacterSet, OtpTextTransform);
        }

        return this with
        {
            Id = Id.Trim(),
            Name = NormalizeText(Name),
            Label = NormalizeText(Label),
            Description = NormalizeText(Description),
            Value = normalizedValue,
            SecondaryValue = NormalizeValue(SecondaryValue),
            Placeholder = NormalizeText(Placeholder),
            Mask = NormalizeText(Mask),
            Prefix = NormalizeText(Prefix),
            Suffix = NormalizeText(Suffix),
            Accept = NormalizeText(Accept),
            ValidationMessage = NormalizeText(ValidationMessage),
            Autocomplete = NormalizeText(Autocomplete),
            Locale = NormalizeLocale(Locale),
            CurrencyCode = NormalizeCurrency(CurrencyCode),
            CodeLength = normalizedCodeLength,
            Rows = Math.Clamp(Rows, 2, 18),
            Choices = Choices.Select(choice => choice with
            {
                Value = choice.Value.Trim(),
                Label = string.IsNullOrWhiteSpace(choice.Label) ? choice.Value.Trim() : choice.Label.Trim(),
                Description = NormalizeText(choice.Description),
                Icon = NormalizeText(choice.Icon),
                Group = NormalizeText(choice.Group)
            }).ToArray(),
            Metadata = Metadata
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Key))
                .ToDictionary(pair => pair.Key.Trim(), pair => NormalizeText(pair.Value), StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <summary>Returns selected values from a compact host value.</summary>
    public IReadOnlySet<string> GetSelectedValues()
        => (Value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the effective number of cells for a verification-code control.</summary>
    public int GetOtpLength() => CodeLength ?? MaximumLength ?? 6;

    private static string? NormalizeText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeValue(string? value)
        => value is null ? null : value.Trim();

    private static string? NormalizeOtpValue(string? value, int length, ZoorceOtpCharacterSet characterSet, ZoorceOtpTextTransform textTransform)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var normalized = value
            .Where(character => characterSet switch
            {
                ZoorceOtpCharacterSet.Numeric => character is >= '0' and <= '9',
                ZoorceOtpCharacterSet.Alphabetic => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z',
                ZoorceOtpCharacterSet.Alphanumeric => character is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z',
                _ => false
            })
            .Take(length)
            .ToArray();

        return textTransform switch
        {
            ZoorceOtpTextTransform.Uppercase => new string(normalized).ToUpperInvariant(),
            ZoorceOtpTextTransform.Lowercase => new string(normalized).ToLowerInvariant(),
            _ => new string(normalized)
        };
    }

    private static string? NormalizeLocale(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try { return CultureInfo.GetCultureInfo(value.Trim()).Name; }
        catch (CultureNotFoundException) { throw new ArgumentException("The locale is not recognized.", nameof(value)); }
    }

    private static string? NormalizeCurrency(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || !normalized.All(char.IsLetter))
        {
            throw new ArgumentException("Currency code must use three letters.", nameof(value));
        }

        return normalized;
    }
}
