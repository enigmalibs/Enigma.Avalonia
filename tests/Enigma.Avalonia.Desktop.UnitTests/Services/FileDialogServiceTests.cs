using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Avalonia.Platform.Storage;
using Enigma.Avalonia.Desktop.Services;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Services;

/// <summary>
/// Covers the file dialog service's initialisation guard and its forwarding to the storage
/// provider, plus everything the convenience extension methods build on top.
/// </summary>
public sealed class FileDialogServiceTests
{
    private static readonly FilePickerFileType TextFiles = new("Text files")
    {
        Patterns = ["*.txt"],
        MimeTypes = ["text/plain"],
    };

    private static readonly FilePickerFileType AllFiles = new("All files") { Patterns = ["*"] };

    [Fact]
    public async Task ShowOpenFileDialogAsync_BeforeTheProviderIsSet_Throws()
    {
        FileDialogService service = new();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ShowOpenFileDialogAsync(new FilePickerOpenOptions()));

        Assert.Equal("Storage provider is not set", exception.Message);
    }

    [Fact]
    public async Task ShowSaveFileDialogAsync_BeforeTheProviderIsSet_Throws()
    {
        FileDialogService service = new();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ShowSaveFileDialogAsync(new FilePickerSaveOptions()));
    }

    [Fact]
    public async Task TheOpenExtension_BeforeTheProviderIsSet_Throws()
    {
        FileDialogService service = new();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ShowOpenFileDialogAsync(title: "Pick a file"));
    }

    [Fact]
    public async Task TheSaveExtension_BeforeTheProviderIsSet_Throws()
    {
        FileDialogService service = new();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ShowSaveFileDialogAsync(title: "Save as"));
    }

    [Fact]
    public void BeforeInitialisation_TheProviderIsNull()
    {
        FileDialogService service = new();

        Assert.Null(service.StorageProvider);
    }

    [AvaloniaFact]
    public void SetStorageProvider_ExposesTheProvider()
    {
        FileDialogService service = new();
        IStorageProvider provider = HeadlessStorage.Provider();

        service.SetStorageProvider(provider);

        Assert.Same(provider, service.StorageProvider);
    }

    /// <summary>
    /// Once initialised the service forwards to the platform provider rather than throwing. The
    /// headless provider cancels every picker, so the call completes with no selection.
    /// </summary>
    [AvaloniaFact]
    public async Task AfterInitialisation_ShowOpenFileDialogAsync_ForwardsToTheProvider()
    {
        FileDialogService service = new();
        service.SetStorageProvider(HeadlessStorage.Provider());

        var files = await service.ShowOpenFileDialogAsync(new FilePickerOpenOptions { Title = "Pick" });

        Assert.Empty(files);
    }

    [AvaloniaFact]
    public async Task AfterInitialisation_ShowSaveFileDialogAsync_ForwardsToTheProvider()
    {
        FileDialogService service = new();
        service.SetStorageProvider(HeadlessStorage.Provider());

        var file = await service.ShowSaveFileDialogAsync(new FilePickerSaveOptions { Title = "Save" });

        Assert.Null(file);
    }

    /// <summary>
    /// The open extension's whole job is assembling <see cref="FilePickerOpenOptions"/> — including
    /// handing the caller's <see cref="FilePickerFileType"/> list through unchanged.
    /// </summary>
    [AvaloniaFact]
    public async Task TheOpenExtension_BuildsTheExpectedOptions()
    {
        RecordingFileDialogService service = new(HeadlessStorage.Provider());

        await service.ShowOpenFileDialogAsync(
            title: "Pick a file",
            allowMultiple: true,
            suggestedFileName: "notes.txt",
            fileTypeFilter: [TextFiles, AllFiles]);

        FilePickerOpenOptions options = Assert.IsType<FilePickerOpenOptions>(service.LastOpenOptions);
        Assert.Equal("Pick a file", options.Title);
        Assert.True(options.AllowMultiple);
        Assert.Equal("notes.txt", options.SuggestedFileName);
        Assert.NotNull(options.FileTypeFilter);
        Assert.Equal(["Text files", "All files"], options.FileTypeFilter.Select(type => type.Name));
        Assert.Equal(["*.txt"], options.FileTypeFilter[0].Patterns ?? []);
        Assert.Equal(["text/plain"], options.FileTypeFilter[0].MimeTypes ?? []);
    }

    /// <summary>With no arguments the extension still produces usable options rather than nulls.</summary>
    [AvaloniaFact]
    public async Task TheOpenExtension_WithNoArguments_LeavesTheOptionsAtTheirDefaults()
    {
        RecordingFileDialogService service = new(HeadlessStorage.Provider());

        await service.ShowOpenFileDialogAsync();

        FilePickerOpenOptions options = Assert.IsType<FilePickerOpenOptions>(service.LastOpenOptions);
        Assert.False(options.AllowMultiple);
        Assert.Null(options.SuggestedFileName);
        Assert.Null(options.FileTypeFilter);
    }

    /// <summary>A start location is resolved through the storage provider into a folder handle.</summary>
    [AvaloniaFact]
    public async Task TheOpenExtension_ResolvesTheSuggestedStartLocation()
    {
        RecordingFileDialogService service = new(HeadlessStorage.Provider());
        string directory = Path.GetTempPath();

        await service.ShowOpenFileDialogAsync(suggestedStartLocation: directory);

        FilePickerOpenOptions options = Assert.IsType<FilePickerOpenOptions>(service.LastOpenOptions);
        Assert.NotNull(options.SuggestedStartLocation);
        Assert.Equal(
            Path.TrimEndingDirectorySeparator(directory),
            Path.TrimEndingDirectorySeparator(options.SuggestedStartLocation.Path.LocalPath));
    }

    [AvaloniaFact]
    public async Task TheSaveExtension_BuildsTheExpectedOptions()
    {
        RecordingFileDialogService service = new(HeadlessStorage.Provider());

        await service.ShowSaveFileDialogAsync(
            title: "Save as",
            suggestedFileName: "report",
            defaultExtension: "pdf",
            showOverwritePrompt: false,
            fileTypeChoices: [TextFiles]);

        FilePickerSaveOptions options = Assert.IsType<FilePickerSaveOptions>(service.LastSaveOptions);
        Assert.Equal("Save as", options.Title);
        Assert.Equal("report", options.SuggestedFileName);
        Assert.Equal("pdf", options.DefaultExtension);
        Assert.False(options.ShowOverwritePrompt);
        Assert.NotNull(options.FileTypeChoices);
        Assert.Equal(["Text files"], options.FileTypeChoices.Select(type => type.Name));
    }

    /// <summary>The overwrite prompt defaults to on — the safe default for a save dialog.</summary>
    [AvaloniaFact]
    public async Task TheSaveExtension_ShowsTheOverwritePromptByDefault()
    {
        RecordingFileDialogService service = new(HeadlessStorage.Provider());

        await service.ShowSaveFileDialogAsync();

        FilePickerSaveOptions options = Assert.IsType<FilePickerSaveOptions>(service.LastSaveOptions);
        Assert.True(options.ShowOverwritePrompt);
        Assert.Null(options.DefaultExtension);
    }

    /// <summary>A cancelled save dialog reports <see langword="null"/> rather than an empty string.</summary>
    [AvaloniaFact]
    public async Task TheSaveExtension_ReturnsNullWhenTheDialogIsCancelled()
    {
        RecordingFileDialogService service = new(HeadlessStorage.Provider()) { SaveResult = null };

        string? path = await service.ShowSaveFileDialogAsync(title: "Save as");

        Assert.Null(path);
    }

    /// <summary>The open extension projects the selected files down to their local paths.</summary>
    [AvaloniaFact]
    public async Task TheOpenExtension_ProjectsTheSelectionToLocalPaths()
    {
        IStorageProvider provider = HeadlessStorage.Provider();
        string filePath = Path.Combine(Path.GetTempPath(), $"enigma-{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(filePath, "x", TestContext.Current.CancellationToken);

        try
        {
            IStorageFile? file = await provider.TryGetFileFromPathAsync(filePath);
            Assert.NotNull(file);
            RecordingFileDialogService service = new(provider) { OpenResult = [file] };

            IEnumerable<string> paths = await service.ShowOpenFileDialogAsync();

            Assert.Equal([filePath], paths);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    /// <summary>An empty selection projects to an empty path list, not to <see langword="null"/>.</summary>
    [AvaloniaFact]
    public async Task TheOpenExtension_ProjectsAnEmptySelectionToAnEmptyList()
    {
        RecordingFileDialogService service = new(HeadlessStorage.Provider());

        IEnumerable<string> paths = await service.ShowOpenFileDialogAsync();

        Assert.Empty(paths);
    }
}
