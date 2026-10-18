using Zoerce.WebUI.Abstractions.Notifications;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Zoerce.WebUI.Sample.Pages;

public class IndexModel(IToastService toasts) : PageModel
{
    public void OnGet()
    {

    }

    public void OnPostServerToast()
    {
        toasts.Show(new ZoorceToastMessage(
            "Server notification",
            "This toast crossed the Razor Pages request boundary successfully.",
            ZoorceToastKind.Information));
    }
}
