using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders a compact multi-select dropdown rather than an always-open choice list.</summary>
public sealed class ZoorceMultiSelectDropdownViewComponent : ViewComponent
{
    /// <summary>Renders the supplied multi-select dropdown configuration.</summary>
    public IViewComponentResult Invoke(ZoorceMultiSelectDropdownOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var control = options.ToRenderOptions();
        return View(options with { Id = control.Id, Name = control.Name, Label = control.Label, Description = control.Description, Value = control.Value, Placeholder = control.Placeholder, State = control.State, ValidationMessage = control.ValidationMessage, Autocomplete = control.Autocomplete, Metadata = control.Metadata, Choices = control.Choices, PlaceholderText = string.IsNullOrWhiteSpace(options.PlaceholderText) ? "Choose options" : options.PlaceholderText.Trim() });
    }
}
