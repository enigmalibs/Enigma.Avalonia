namespace Enigma.Avalonia.Desktop.Controls.InfoBar;

/// <summary>
/// Specifies the visual severity of an <see cref="InfoBar"/> message.
/// </summary>
public enum InfoBarSeverity
{
    /// <summary>Informational message with neutral styling.</summary>
    Info,

    /// <summary>Success message with positive styling.</summary>
    Success,

    /// <summary>Warning message indicating a potential issue.</summary>
    Warning,

    /// <summary>Error message indicating a failure or critical problem.</summary>
    Error
}
