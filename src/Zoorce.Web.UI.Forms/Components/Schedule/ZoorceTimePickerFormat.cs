namespace Zoorce.Web.UI.Forms;

/// <summary>Controls the clock notation rendered by a <see cref="ZoorceTimePickerOptions"/>.</summary>
public enum ZoorceTimePickerFormat
{
    /// <summary>Uses the active locale's preferred clock notation.</summary>
    Locale,

    /// <summary>Always renders a twelve-hour clock with AM and PM.</summary>
    TwelveHour,

    /// <summary>Always renders a twenty-four-hour clock.</summary>
    TwentyFourHour
}
