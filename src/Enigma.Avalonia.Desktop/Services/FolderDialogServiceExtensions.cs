using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Provides convenient extension methods for <see cref="IFolderDialogService"/> that simplify common folder dialog operations.
/// These methods automatically create and configure dialog options and return folder paths as strings.
/// </summary>
public static class FolderDialogServiceExtensions
{
    extension(IFolderDialogService service)
    {
        /// <summary>
        /// Shows an open folder dialog with simplified parameters and returns the selected folder paths as strings.
        /// </summary>
        /// <param name="title">The title to display in the dialog window.</param>
        /// <param name="allowMultiple">Whether to allow selecting multiple folders.</param>
        /// <param name="suggestedStartLocation">The initial directory path to display in the dialog.</param>
        /// <param name="suggestedFileName">The default folder name to display in the dialog.</param>
        /// <returns>A task that represents the asynchronous operation, containing the paths of selected folders as strings.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no storage provider has been set.</exception>
        public async Task<IEnumerable<string>> ShowOpenFolderDialogAsync(
            string? title = null,
            bool allowMultiple = false,
            string? suggestedStartLocation = null,
            string? suggestedFileName = null)
        {
            if (service.StorageProvider is null)
                throw new InvalidOperationException("Storage provider is not set");

            var options = new FolderPickerOpenOptions();

            if (title is not null)
                options.Title = title;
            options.AllowMultiple = allowMultiple;
            if (suggestedStartLocation is not null)
                options.SuggestedStartLocation =
                    await service.StorageProvider.TryGetFolderFromPathAsync(suggestedStartLocation);
            if (suggestedFileName is not null)
                options.SuggestedFileName = suggestedFileName;

            var folders = await service.ShowOpenFolderDialogAsync(options);

            List<string> paths = [];
            paths.AddRange(folders.Select(folder => folder.TryGetLocalPath()).OfType<string>());

            return paths;
        }
    }
}
