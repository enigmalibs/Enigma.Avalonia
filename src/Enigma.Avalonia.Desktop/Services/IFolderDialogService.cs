using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Defines a service for displaying folder picker dialogs in the application.
/// The storage provider must be set via <see cref="SetStorageProvider"/> before showing dialogs.
/// </summary>
public interface IFolderDialogService
{
    /// <summary>
    /// Gets the registered storage provider used to display folder dialogs.
    /// </summary>
    IStorageProvider? StorageProvider { get; }

    /// <summary>
    /// Sets the storage provider that will be used to display all folder dialogs.
    /// This must be called once during application initialization before showing any dialogs.
    /// </summary>
    /// <param name="storageProvider">The storage provider to register.</param>
    void SetStorageProvider(IStorageProvider storageProvider);

    /// <summary>
    /// Shows an open folder dialog with the specified options.
    /// </summary>
    /// <param name="options">The options that configure the folder picker dialog.</param>
    /// <returns>A task that represents the asynchronous operation, containing the list of selected folders.</returns>
    Task<IReadOnlyList<IStorageFolder>> ShowOpenFolderDialogAsync(FolderPickerOpenOptions options);
}
