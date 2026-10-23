using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders a standalone dropdown tree field.</summary>
public sealed class ZoorceDropdownTreeViewComponent : ViewComponent
{
    /// <summary>Renders the supplied dropdown tree configuration.</summary>
    public IViewComponentResult Invoke(ZoorceDropdownTreeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var control = options.ToRenderOptions();
        return View(options with { Id = control.Id, Name = control.Name, Label = control.Label, Description = control.Description, Value = control.Value, Placeholder = control.Placeholder, State = control.State, ValidationMessage = control.ValidationMessage, Autocomplete = control.Autocomplete, Metadata = control.Metadata, PlaceholderText = string.IsNullOrWhiteSpace(options.PlaceholderText) ? "Choose a node" : options.PlaceholderText.Trim() });
    }
}
