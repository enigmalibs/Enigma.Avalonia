namespace Enigma.Avalonia.Desktop.Controls.ContentDialog;

/// <summary>
/// Specifies which button in a <see cref="ContentDialog"/> is styled as the default action.
/// </summary>
public enum DefaultButton
{
    /// <summary>No button is styled as the default.</summary>
    None,

    /// <summary>The primary button is styled as the default.</summary>
    Primary,

    /// <summary>The secondary button is styled as the default.</summary>
    Secondary,

    /// <summary>The close button is styled as the default.</summary>
    Close
}
