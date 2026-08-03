using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Avalonia.Platform.Storage;
using Enigma.Avalonia.Desktop.Services;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Services;

/// <summary>
/// Covers the folder dialog service's initialisation guard, its forwarding to the storage provider,
/// and the options its convenience extension builds.
/// </summary>
public sealed class FolderDialogServiceTests
{
    [Fact]
    public async Task ShowOpenFolderDialogAsync_BeforeTheProviderIsSet_Throws()
    {
        FolderDialogService service = new();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ShowOpenFolderDialogAsync(new FolderPickerOpenOptions()));

        Assert.Equal("Storage provider is not set", exception.Message);
    }

    [Fact]
    public async Task TheOpenExtension_BeforeTheProviderIsSet_Throws()
    {
        FolderDialogService service = new();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ShowOpenFolderDialogAsync(title: "Pick a folder"));
    }

    [Fact]
    public void BeforeInitialisation_TheProviderIsNull()
    {
        FolderDialogService service = new();

        Assert.Null(service.StorageProvider);
    }

    [AvaloniaFact]
    public void SetStorageProvider_ExposesTheProvider()
    {
        FolderDialogService service = new();
        IStorageProvider provider = HeadlessStorage.Provider();

        service.SetStorageProvider(provider);

        Assert.Same(provider, service.StorageProvider);
    }

    [AvaloniaFact]
    public async Task AfterInitialisation_ShowOpenFolderDialogAsync_ForwardsToTheProvider()
    {
        FolderDialogService service = new();
        service.SetStorageProvider(HeadlessStorage.Provider());

        var folders = await service.ShowOpenFolderDialogAsync(
            new FolderPickerOpenOptions { Title = "Pick" });

        Assert.Empty(folders);
    }

    [AvaloniaFact]
    public async Task TheOpenExtension_BuildsTheExpectedOptions()
    {
        RecordingFolderDialogService service = new(HeadlessStorage.Provider());

        await service.ShowOpenFolderDialogAsync(
            title: "Pick a folder",
            allowMultiple: true,
            suggestedFileName: "Projects");

        FolderPickerOpenOptions options = Assert.IsType<FolderPickerOpenOptions>(service.LastOpenOptions);
        Assert.Equal("Pick a folder", options.Title);
        Assert.True(options.AllowMultiple);
        Assert.Equal("Projects", options.SuggestedFileName);
    }

    [AvaloniaFact]
    public async Task TheOpenExtension_WithNoArguments_LeavesTheOptionsAtTheirDefaults()
    {
        RecordingFolderDialogService service = new(HeadlessStorage.Provider());

        await service.ShowOpenFolderDialogAsync();

        FolderPickerOpenOptions options = Assert.IsType<FolderPickerOpenOptions>(service.LastOpenOptions);
        Assert.False(options.AllowMultiple);
        Assert.Null(options.SuggestedFileName);
        Assert.Null(options.SuggestedStartLocation);
    }

    [AvaloniaFact]
    public async Task TheOpenExtension_ResolvesTheSuggestedStartLocation()
    {
        RecordingFolderDialogService service = new(HeadlessStorage.Provider());
        string directory = Path.GetTempPath();

        await service.ShowOpenFolderDialogAsync(suggestedStartLocation: directory);

        FolderPickerOpenOptions options = Assert.IsType<FolderPickerOpenOptions>(service.LastOpenOptions);
        Assert.NotNull(options.SuggestedStartLocation);
        Assert.Equal(
            Path.TrimEndingDirectorySeparator(directory),
            Path.TrimEndingDirectorySeparator(options.SuggestedStartLocation.Path.LocalPath));
    }

    /// <summary>The extension projects the selected folders down to their local paths.</summary>
    [AvaloniaFact]
    public async Task TheOpenExtension_ProjectsTheSelectionToLocalPaths()
    {
        IStorageProvider provider = HeadlessStorage.Provider();
        string directory = Path.Combine(Path.GetTempPath(), $"enigma-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            IStorageFolder? folder = await provider.TryGetFolderFromPathAsync(directory);
            Assert.NotNull(folder);
            RecordingFolderDialogService service = new(provider) { OpenResult = [folder] };

            IEnumerable<string> paths = await service.ShowOpenFolderDialogAsync();

            Assert.Equal([directory], paths);
        }
        finally
        {
            Directory.Delete(directory);
        }
    }

    [AvaloniaFact]
    public async Task TheOpenExtension_ProjectsAnEmptySelectionToAnEmptyList()
    {
        RecordingFolderDialogService service = new(HeadlessStorage.Provider());

        IEnumerable<string> paths = await service.ShowOpenFolderDialogAsync();

        Assert.Empty(paths);
    }
}
