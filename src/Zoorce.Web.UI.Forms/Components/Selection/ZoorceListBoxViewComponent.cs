using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders a standalone accessible listbox.</summary>
public sealed class ZoorceListBoxViewComponent : ViewComponent
{
    /// <summary>Renders the supplied listbox configuration.</summary>
    public IViewComponentResult Invoke(ZoorceListBoxOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var control = options.ToRenderOptions();
        return View(options with { Id = control.Id, Name = control.Name, Label = control.Label, Description = control.Description, Value = control.Value, Placeholder = control.Placeholder, State = control.State, ValidationMessage = control.ValidationMessage, Autocomplete = control.Autocomplete, Metadata = control.Metadata, Choices = control.Choices, MaxHeight = Math.Clamp(options.MaxHeight, 100, 720), EmptyText = string.IsNullOrWhiteSpace(options.EmptyText) ? "No options available." : options.EmptyText.Trim() });
    }
}
