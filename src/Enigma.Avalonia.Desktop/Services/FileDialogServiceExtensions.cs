using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Provides convenient extension methods for <see cref="IFileDialogService"/> that simplify common file dialog operations.
/// These methods automatically create and configure dialog options and return file paths as strings.
/// </summary>
public static class FileDialogServiceExtensions
{
    extension(IFileDialogService service)
    {
        /// <summary>
        /// Shows an open file dialog with simplified parameters and returns the selected file paths as strings.
        /// </summary>
        /// <param name="title">The title to display in the dialog window.</param>
        /// <param name="allowMultiple">Whether to allow selecting multiple files.</param>
        /// <param name="suggestedStartLocation">The initial directory path to display in the dialog.</param>
        /// <param name="suggestedFileName">The default file name to display in the dialog.</param>
        /// <param name="fileTypeFilter">The list of file type filters to apply (e.g., only show .txt files).</param>
        /// <returns>A task that represents the asynchronous operation, containing the paths of selected files as strings.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no storage provider has been set.</exception>
        public async Task<IEnumerable<string>> ShowOpenFileDialogAsync(
            string? title = null,
            bool allowMultiple = false,
            string? suggestedStartLocation = null,
            string? suggestedFileName = null,
            IReadOnlyList<FilePickerFileType>? fileTypeFilter = null)
        {
            if (service.StorageProvider is null)
                throw new InvalidOperationException("Storage provider is not set");

            var options = new FilePickerOpenOptions();

            if (title is not null)
                options.Title = title;
            options.AllowMultiple = allowMultiple;
            if (suggestedStartLocation is not null)
                options.SuggestedStartLocation =
                    await service.StorageProvider.TryGetFolderFromPathAsync(suggestedStartLocation);
            if (suggestedFileName is not null)
                options.SuggestedFileName = suggestedFileName;
            options.FileTypeFilter = fileTypeFilter;

            var files = await service.ShowOpenFileDialogAsync(options);

            List<string> paths = [];
            paths.AddRange(files.Select(folder => folder.TryGetLocalPath()).OfType<string>());

            return paths;
        }

        /// <summary>
        /// Shows a save file dialog with simplified parameters and returns the selected file path as a string.
        /// </summary>
        /// <param name="title">The title to display in the dialog window.</param>
        /// <param name="suggestedStartLocation">The initial directory path to display in the dialog.</param>
        /// <param name="suggestedFileName">The default file name to display in the dialog.</param>
        /// <param name="defaultExtension">The default file extension to append if the user doesn't specify one.</param>
        /// <param name="showOverwritePrompt">Whether to show a confirmation prompt if the selected file already exists.</param>
        /// <param name="fileTypeChoices">The list of file type options to present in the save dialog.</param>
        /// <returns>A task that represents the asynchronous operation, containing the selected file path as a string, or null if cancelled.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no storage provider has been set.</exception>
        public async Task<string?> ShowSaveFileDialogAsync(
            string? title = null,
            string? suggestedStartLocation = null,
            string? suggestedFileName = null,
            string? defaultExtension = null,
            bool showOverwritePrompt = true,
            IReadOnlyList<FilePickerFileType>? fileTypeChoices = null)
        {
            if (service.StorageProvider is null)
                throw new InvalidOperationException("Storage provider is not set");

            var options = new FilePickerSaveOptions();

            if (title is not null)
                options.Title = title;
            if (suggestedStartLocation is not null)
                options.SuggestedStartLocation =
                    await service.StorageProvider.TryGetFolderFromPathAsync(suggestedStartLocation);
            if (suggestedFileName is not null)
                options.SuggestedFileName = suggestedFileName;
            if (defaultExtension is not null)
                options.DefaultExtension = defaultExtension;
            options.ShowOverwritePrompt = showOverwritePrompt;
            if (fileTypeChoices is not null)
                options.FileTypeChoices = fileTypeChoices;

            var file = await service.ShowSaveFileDialogAsync(options);
            return file?.TryGetLocalPath();
        }
    }
}
