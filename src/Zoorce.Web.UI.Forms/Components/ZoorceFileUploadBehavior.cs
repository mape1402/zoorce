namespace Zoorce.Web.UI.Forms;

/// <summary>Defines when a selected file is sent to an optional upload endpoint.</summary>
public enum ZoorceFileUploadBehavior
{
    /// <summary>Keeps the files in the native input so the surrounding form can submit them normally.</summary>
    Deferred,

    /// <summary>Starts an upload as soon as a valid file is selected.</summary>
    Automatic,

    /// <summary>Waits for the user to press the upload action rendered by the control.</summary>
    Manual
}
