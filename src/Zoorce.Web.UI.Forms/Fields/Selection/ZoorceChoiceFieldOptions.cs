using Microsoft.AspNetCore.Html;

namespace Zoorce.Web.UI.Forms;

/// <summary>Provides shared choices configuration for selectable form fields.</summary>
public abstract record ZoorceChoiceFieldOptions : ZoorceFieldOptions
{
    /// <summary>Gets the choices offered by the field.</summary>
    public IReadOnlyList<ZoorceFormChoice> Choices { get; init; } = [];

    /// <summary>Gets whether a visitor may create a value outside the configured choices.</summary>
    public bool AllowCustomValues { get; init; }

    /// <summary>
    /// Gets optional host markup used to render an individual choice in controls that support rich option content.
    /// </summary>
    /// <remarks>
    /// The template is invoked only for visible choice content. The control retains ownership of selection,
    /// keyboard behavior, accessibility attributes, and the submitted value.
    /// </remarks>
    public Func<ZoorceFormChoice, IHtmlContent>? ChoiceTemplate { get; init; }

    /// <summary>Creates choice settings shared by a concrete selectable field.</summary>
    private protected ZoorceFormControlOptions CreateChoiceRenderOptions()
        => base.CreateRenderOptions() with
        {
            Choices = Choices,
            AllowCustomValues = AllowCustomValues,
            ChoiceTemplate = ChoiceTemplate
        };
}
