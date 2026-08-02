using System;
using System.Threading.Tasks;
using Enigma.Avalonia.Desktop.Controls.ContentDialog;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Defines a service for displaying and managing content dialogs in the application.
/// ContentDialog instances must be registered via <see cref="RegisterHost"/> before showing dialogs.
/// </summary>
public interface IContentDialogService
{
    /// <summary>
    /// Registers the host ContentDialog control that will be used to display all dialogs.
    /// This must be called once during application initialization before showing any dialogs.
    /// </summary>
    /// <param name="dialog">The ContentDialog control to register as the host.</param>
    void RegisterHost(ContentDialog dialog);

    /// <summary>
    /// Shows a simple message dialog with a title, message, and close button.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message to display in the dialog.</param>
    /// <param name="closeButtonText">The text for the close button (defaults to "OK").</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the dialog.</returns>
    Task<DialogResult> ShowMessageAsync(string title, string message, string closeButtonText = "OK");

    /// <summary>
    /// Shows a fully customizable content dialog.
    /// The configuration action receives the ContentDialog instance to configure before showing.
    /// </summary>
    /// <param name="configure">An action that configures the ContentDialog properties before showing it.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the dialog.</returns>
    Task<DialogResult> ShowAsync(Action<ContentDialog> configure);

    /// <summary>
    /// Hides the currently displayed content dialog, if any.
    /// </summary>
    /// <returns>A task that represents the asynchronous hide operation.</returns>
    Task HideAsync();
}
