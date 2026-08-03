using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Enigma.Avalonia.Desktop.Services;

namespace Enigma.Avalonia.Desktop.UnitTests.Services;

/// <summary>
/// Supplies the real <see cref="IStorageProvider"/> a headless <see cref="Window"/> exposes.
/// </summary>
/// <remarks>
/// Avalonia 12 marks <see cref="IStorageProvider"/>, <see cref="IStorageFile"/> and
/// <see cref="IStorageFolder"/> as not client-implementable, so a hand-written stub will not
/// compile. The headless platform's BCL-backed provider is the only provider a test can obtain,
/// and it is enough: the pickers return empty results and path lookups work against the real
/// file system.
/// </remarks>
internal static class HeadlessStorage
{
    /// <summary>Shows a headless window and returns its storage provider.</summary>
    /// <returns>A usable storage provider.</returns>
    public static IStorageProvider Provider()
    {
        Window window = new();
        window.Show();
        return window.StorageProvider;
    }
}

/// <summary>
/// An <see cref="IFileDialogService"/> that records the options it is handed instead of showing a
/// dialog, so the convenience extension methods can be tested on what they build.
/// </summary>
/// <param name="storageProvider">
/// The provider the extensions see. Pass <see langword="null"/> to exercise the
/// "storage provider is not set" guard.
/// </param>
internal sealed class RecordingFileDialogService(IStorageProvider? storageProvider) : IFileDialogService
{
    /// <inheritdoc />
    public IStorageProvider? StorageProvider { get; private set; } = storageProvider;

    /// <summary>Gets the options the last open-file call was built with.</summary>
    public FilePickerOpenOptions? LastOpenOptions { get; private set; }

    /// <summary>Gets the options the last save-file call was built with.</summary>
    public FilePickerSaveOptions? LastSaveOptions { get; private set; }

    /// <summary>Gets or sets the files the open dialog reports back.</summary>
    public IReadOnlyList<IStorageFile> OpenResult { get; set; } = [];

    /// <summary>Gets or sets the file the save dialog reports back, or <see langword="null"/> when cancelled.</summary>
    public IStorageFile? SaveResult { get; set; }

    /// <inheritdoc />
    public void SetStorageProvider(IStorageProvider storageProvider) => StorageProvider = storageProvider;

    /// <inheritdoc />
    public Task<IReadOnlyList<IStorageFile>> ShowOpenFileDialogAsync(FilePickerOpenOptions options)
    {
        LastOpenOptions = options;
        return Task.FromResult(OpenResult);
    }

    /// <inheritdoc />
    public Task<IStorageFile?> ShowSaveFileDialogAsync(FilePickerSaveOptions options)
    {
        LastSaveOptions = options;
        return Task.FromResult(SaveResult);
    }
}

/// <summary>
/// An <see cref="IFolderDialogService"/> that records the options it is handed instead of showing a
/// dialog.
/// </summary>
/// <param name="storageProvider">
/// The provider the extension sees. Pass <see langword="null"/> to exercise the
/// "storage provider is not set" guard.
/// </param>
internal sealed class RecordingFolderDialogService(IStorageProvider? storageProvider) : IFolderDialogService
{
    /// <inheritdoc />
    public IStorageProvider? StorageProvider { get; private set; } = storageProvider;

    /// <summary>Gets the options the last open-folder call was built with.</summary>
    public FolderPickerOpenOptions? LastOpenOptions { get; private set; }

    /// <summary>Gets or sets the folders the dialog reports back.</summary>
    public IReadOnlyList<IStorageFolder> OpenResult { get; set; } = [];

    /// <inheritdoc />
    public void SetStorageProvider(IStorageProvider storageProvider) => StorageProvider = storageProvider;

    /// <inheritdoc />
    public Task<IReadOnlyList<IStorageFolder>> ShowOpenFolderDialogAsync(FolderPickerOpenOptions options)
    {
        LastOpenOptions = options;
        return Task.FromResult(OpenResult);
    }
}
