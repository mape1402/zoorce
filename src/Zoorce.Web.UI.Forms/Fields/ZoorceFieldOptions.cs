namespace Zoorce.Web.UI.Forms;

/// <summary>
/// Supplies the common, host-configurable contract shared by every named Zoorce form field.
/// </summary>
/// <remarks>
/// Concrete field options add only the settings meaningful to that field and convert themselves
/// into the internal rendering model. Hosts should configure the concrete option type rather than
/// a discriminator or a switch statement.
/// </remarks>
public abstract record ZoorceFieldOptions
{
    /// <summary>Gets the unique DOM identifier for the field.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the standard HTML form name.</summary>
    public string? Name { get; init; }

    /// <summary>Gets the visible field label.</summary>
    public string? Label { get; init; }

    /// <summary>Gets supporting copy rendered with the field.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the primary submitted value.</summary>
    public string? Value { get; init; }

    /// <summary>Gets the placeholder shown by entry fields.</summary>
    public string? Placeholder { get; init; }

    /// <summary>Gets whether the field requires a value.</summary>
    public bool Required { get; init; }

    /// <summary>Gets whether the field is disabled.</summary>
    public bool Disabled { get; init; }

    /// <summary>Gets whether the field is visible but immutable.</summary>
    public bool ReadOnly { get; init; }

    /// <summary>Gets host-provided validation state.</summary>
    public ZoorceFormControlState State { get; init; } = ZoorceFormControlState.Default;

    /// <summary>Gets host-provided validation copy.</summary>
    public string? ValidationMessage { get; init; }

    /// <summary>Gets the browser autocomplete token.</summary>
    public string? Autocomplete { get; init; }

    /// <summary>Gets host metadata exposed to an individual field template.</summary>
    public IReadOnlyDictionary<string, string?> Metadata { get; init; } = new Dictionary<string, string?>();

    /// <summary>Creates the normalized rendering configuration for the concrete field.</summary>
    internal ZoorceFormControlOptions ToRenderOptions()
        => CreateRenderOptions().Normalize();

    /// <summary>Creates the concrete field rendering configuration.</summary>
    private protected virtual ZoorceFormControlOptions CreateRenderOptions()
        => new()
        {
            Id = Id,
            Name = Name,
            Label = Label,
            Description = Description,
            Value = Value,
            Placeholder = Placeholder,
            Required = Required,
            Disabled = Disabled,
            ReadOnly = ReadOnly,
            State = State,
            ValidationMessage = ValidationMessage,
            Autocomplete = Autocomplete,
            Metadata = Metadata
        };
}
