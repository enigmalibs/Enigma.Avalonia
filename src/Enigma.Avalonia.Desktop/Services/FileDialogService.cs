using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Implementation of the file dialog service for displaying file picker dialogs in the application.
/// Wraps the platform's storage provider to provide file open and save dialogs.
/// </summary>
public class FileDialogService : IFileDialogService
{
    /// <summary>
    /// Gets the registered storage provider used to display file dialogs.
    /// </summary>
    public IStorageProvider? StorageProvider { get; private set; }

    /// <summary>
    /// Sets the storage provider that will be used to display all file dialogs.
    /// This must be called once during application initialization before showing any dialogs.
    /// </summary>
    /// <param name="storageProvider">The storage provider to register.</param>
    public void SetStorageProvider(IStorageProvider storageProvider)
        => StorageProvider = storageProvider;

    /// <summary>
    /// Shows an open file dialog with the specified options.
    /// </summary>
    /// <param name="options">The options that configure the file picker dialog.</param>
    /// <returns>A task that represents the asynchronous operation, containing the list of selected files.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no storage provider has been set.</exception>
    public async Task<IReadOnlyList<IStorageFile>> ShowOpenFileDialogAsync(FilePickerOpenOptions options)
    {
        if (StorageProvider is null)
            throw new InvalidOperationException("Storage provider is not set");
        return await StorageProvider.OpenFilePickerAsync(options);
    }

    /// <summary>
    /// Shows a save file dialog with the specified options.
    /// </summary>
    /// <param name="options">The options that configure the file save dialog.</param>
    /// <returns>A task that represents the asynchronous operation, containing the selected file location or null if cancelled.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no storage provider has been set.</exception>
    public async Task<IStorageFile?> ShowSaveFileDialogAsync(FilePickerSaveOptions options)
    {
        if (StorageProvider is null)
            throw new InvalidOperationException("Storage provider is not set");
        return await StorageProvider.SaveFilePickerAsync(options);
    }
}
