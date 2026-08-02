using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Defines a service for displaying file picker dialogs (open and save) in the application.
/// The storage provider must be set via <see cref="SetStorageProvider"/> before showing dialogs.
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// Gets the registered storage provider used to display file dialogs.
    /// </summary>
    IStorageProvider? StorageProvider { get; }

    /// <summary>
    /// Sets the storage provider that will be used to display all file dialogs.
    /// This must be called once during application initialization before showing any dialogs.
    /// </summary>
    /// <param name="storageProvider">The storage provider to register.</param>
    void SetStorageProvider(IStorageProvider storageProvider);

    /// <summary>
    /// Shows an open file dialog with the specified options.
    /// </summary>
    /// <param name="options">The options that configure the file picker dialog.</param>
    /// <returns>A task that represents the asynchronous operation, containing the list of selected files.</returns>
    Task<IReadOnlyList<IStorageFile>> ShowOpenFileDialogAsync(FilePickerOpenOptions options);

    /// <summary>
    /// Shows a save file dialog with the specified options.
    /// </summary>
    /// <param name="options">The options that configure the file save dialog.</param>
    /// <returns>A task that represents the asynchronous operation, containing the selected file location or null if cancelled.</returns>
    Task<IStorageFile?> ShowSaveFileDialogAsync(FilePickerSaveOptions options);
}
