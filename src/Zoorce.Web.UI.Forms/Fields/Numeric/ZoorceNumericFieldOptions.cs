namespace Zoorce.Web.UI.Forms;

/// <summary>Provides shared bounds and adornments for numeric form fields.</summary>
public abstract record ZoorceNumericFieldOptions : ZoorceFieldOptions
{
    /// <summary>Gets the smallest accepted value.</summary>
    public decimal? Minimum { get; init; }

    /// <summary>Gets the largest accepted value.</summary>
    public decimal? Maximum { get; init; }

    /// <summary>Gets the increment applied by increment and decrement controls.</summary>
    public decimal? Step { get; init; }

    /// <summary>Gets optional leading display content.</summary>
    public string? Prefix { get; init; }

    /// <summary>Gets optional trailing display content.</summary>
    public string? Suffix { get; init; }

    /// <summary>Creates numeric settings shared by the concrete numeric control.</summary>
    private protected ZoorceFormControlOptions CreateNumericRenderOptions()
        => base.CreateRenderOptions() with
        {
            Minimum = Minimum,
            Maximum = Maximum,
            Step = Step,
            Prefix = Prefix,
            Suffix = Suffix
        };
}
