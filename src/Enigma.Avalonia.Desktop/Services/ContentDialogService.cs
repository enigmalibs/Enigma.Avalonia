using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Enigma.Avalonia.Desktop.Controls.ContentDialog;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Implementation of the content dialog service for displaying modal dialogs in the application.
/// Manages a single ContentDialog host control and resets it between uses.
/// </summary>
public class ContentDialogService : IContentDialogService
{
    /// <summary>
    /// The registered ContentDialog control used to display all dialogs.
    /// </summary>
    private ContentDialog? _host;

    /// <summary>
    /// Registers the host ContentDialog control that will be used to display all dialogs.
    /// This must be called once during application initialization before showing any dialogs.
    /// </summary>
    /// <param name="dialog">The ContentDialog control to register as the host.</param>
    public void RegisterHost(ContentDialog dialog)
    {
        _host = dialog;
    }

    /// <summary>
    /// Shows a simple message dialog with a title, message, and close button.
    /// The message is displayed in a TextBlock with text wrapping enabled.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message to display in the dialog.</param>
    /// <param name="closeButtonText">The text for the close button (defaults to "OK").</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the dialog.</returns>
    public async Task<DialogResult> ShowMessageAsync(string title, string message, string closeButtonText = "OK")
    {
        return await ShowAsync(dialog =>
        {
            dialog.Title = title;
            dialog.Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };
            dialog.CloseButtonText = closeButtonText;
        });
    }

    /// <summary>
    /// Shows a fully customizable content dialog.
    /// The dialog is reset to default state before the configuration action is applied.
    /// </summary>
    /// <param name="configure">An action that configures the ContentDialog properties before showing it.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the dialog.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no ContentDialog host has been registered.</exception>
    public async Task<DialogResult> ShowAsync(Action<ContentDialog> configure)
    {
        if (_host is null)
            throw new InvalidOperationException("ContentDialog host has not been registered. Call RegisterHost first.");

        ResetDialog(_host);
        configure(_host);
        return await _host.ShowAsync();
    }

    /// <summary>
    /// Hides the currently displayed content dialog, if any.
    /// </summary>
    /// <returns>A task that represents the asynchronous hide operation.</returns>
    public async Task HideAsync()
    {
        if (_host is not null)
            await _host.HideAsync();
    }

    /// <summary>
    /// Resets all properties of the specified ContentDialog to their default values.
    /// This ensures each dialog starts with a clean state.
    /// </summary>
    /// <param name="dialog">The ContentDialog to reset.</param>
    private static void ResetDialog(ContentDialog dialog)
    {
        dialog.Title = null;
        dialog.Content = null;
        dialog.PrimaryButtonText = null;
        dialog.SecondaryButtonText = null;
        dialog.CloseButtonText = null;
        dialog.PrimaryButtonCommand = null;
        dialog.SecondaryButtonCommand = null;
        dialog.CloseButtonCommand = null;
        dialog.IsPrimaryButtonEnabled = true;
        dialog.IsSecondaryButtonEnabled = true;
        dialog.IsCloseButtonEnabled = true;
        dialog.DefaultButton = DefaultButton.None;
        dialog.IconData = null;
        dialog.ClearValue(ContentDialog.IconBrushProperty); // Clear to restore theme default
    }
}
