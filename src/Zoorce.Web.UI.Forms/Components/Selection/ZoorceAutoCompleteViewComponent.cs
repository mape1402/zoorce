using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an independent autocomplete control.</summary>
public sealed class ZoorceAutoCompleteViewComponent : ViewComponent
{
    /// <summary>Renders the supplied autocomplete configuration.</summary>
    public IViewComponentResult Invoke(ZoorceAutoCompleteOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var control = options.ToRenderOptions();
        return View(options with { Id = control.Id, Name = control.Name, Label = control.Label, Description = control.Description, Value = control.Value, Placeholder = control.Placeholder, State = control.State, ValidationMessage = control.ValidationMessage, Autocomplete = control.Autocomplete, Metadata = control.Metadata, Choices = control.Choices, Source = string.IsNullOrWhiteSpace(options.Source) ? null : options.Source.Trim(), MinimumCharacters = Math.Max(0, options.MinimumCharacters), DebounceMilliseconds = Math.Clamp(options.DebounceMilliseconds, 0, 2000), EmptyResultsText = string.IsNullOrWhiteSpace(options.EmptyResultsText) ? "No matching results." : options.EmptyResultsText.Trim() });
    }
}
