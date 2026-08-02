namespace Enigma.Avalonia.Desktop.Controls.ContentDialog;

/// <summary>
/// Specifies the result of a <see cref="ContentDialog"/> interaction.
/// </summary>
public enum DialogResult
{
    /// <summary>No button was pressed; the dialog was dismissed without a definitive choice.</summary>
    None,

    /// <summary>The primary action button was pressed.</summary>
    Primary,

    /// <summary>The secondary action button was pressed.</summary>
    Secondary,

    /// <summary>The close button was pressed.</summary>
    Close
}
