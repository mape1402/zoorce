namespace Zoorce.Web.UI.Forms;

/// <summary>Communicates server or host validation state without coupling a control to a validation framework.</summary>
public enum ZoorceFormControlState
{
    /// <summary>Represents the <c>Default</c> option.</summary>
    Default,
    /// <summary>Represents the <c>Success</c> option.</summary>
    Success,
    /// <summary>Represents the <c>Warning</c> option.</summary>
    Warning,
    /// <summary>Represents the <c>Error</c> option.</summary>
    Error
}
