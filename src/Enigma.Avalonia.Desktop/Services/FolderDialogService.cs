using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Implementation of the folder dialog service for displaying folder picker dialogs in the application.
/// Wraps the platform's storage provider to provide folder selection dialogs.
/// </summary>
public class FolderDialogService : IFolderDialogService
{
    /// <summary>
    /// Gets the registered storage provider used to display folder dialogs.
    /// </summary>
    public IStorageProvider? StorageProvider { get; private set; }

    /// <summary>
    /// Sets the storage provider that will be used to display all folder dialogs.
    /// This must be called once during application initialization before showing any dialogs.
    /// </summary>
    /// <param name="storageProvider">The storage provider to register.</param>
    public void SetStorageProvider(IStorageProvider storageProvider)
        => StorageProvider = storageProvider;

    /// <summary>
    /// Shows an open folder dialog with the specified options.
    /// </summary>
    /// <param name="options">The options that configure the folder picker dialog.</param>
    /// <returns>A task that represents the asynchronous operation, containing the list of selected folders.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no storage provider has been set.</exception>
    public async Task<IReadOnlyList<IStorageFolder>> ShowOpenFolderDialogAsync(FolderPickerOpenOptions options)
    {
        if (StorageProvider is null)
            throw new InvalidOperationException("Storage provider is not set");
        return await StorageProvider.OpenFolderPickerAsync(options);
    }
}
