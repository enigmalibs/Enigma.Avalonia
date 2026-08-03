using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Enigma.Avalonia.Desktop.Services;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the file and folder picker page.</summary>
/// <remarks>
/// <para>
/// The two picker services are the library's answer to a problem every Avalonia MVVM app runs into:
/// <c>IStorageProvider</c> hangs off the window, so a ViewModel that opens a file dialog normally has
/// to reach for the window. Here it takes <see cref="IFileDialogService"/> and
/// <see cref="IFolderDialogService"/> from the container and never sees a <c>Window</c> —
/// <c>App.axaml.cs</c> handed each service the storage provider once, at startup.
/// </para>
/// <para>
/// Every command below calls the string-based convenience overloads from
/// <c>FileDialogServiceExtensions</c> / <c>FolderDialogServiceExtensions</c>, which build the
/// <c>FilePickerOpenOptions</c> for you and hand back local paths. The <c>FilePickerFileType</c>
/// filters show the escape hatch: anything the overloads don't cover, you pass through.
/// </para>
/// </remarks>
public class DialogsTestingPageViewModel : ObservableObject
{
    /// <summary>The file picker service.</summary>
    private readonly IFileDialogService _fileDialogService;

    /// <summary>The folder picker service.</summary>
    private readonly IFolderDialogService _folderDialogService;

    /// <summary>Initializes a new instance of the <see cref="DialogsTestingPageViewModel"/> class.</summary>
    /// <param name="fileDialogService">The file picker service.</param>
    /// <param name="folderDialogService">The folder picker service.</param>
    public DialogsTestingPageViewModel(
        IFileDialogService fileDialogService,
        IFolderDialogService folderDialogService)
    {
        _fileDialogService = fileDialogService;
        _folderDialogService = folderDialogService;

        OpenSingleFileCommand = new AsyncRelayCommand(OnOpenSingleFileAsync);
        OpenMultipleFilesCommand = new AsyncRelayCommand(OnOpenMultipleFilesAsync);
        OpenTextFilesCommand = new AsyncRelayCommand(OnOpenTextFilesAsync);
        SaveFileCommand = new AsyncRelayCommand(OnSaveFileAsync);
        SaveFileWithExtensionCommand = new AsyncRelayCommand(OnSaveFileWithExtensionAsync);

        OpenSingleFolderCommand = new AsyncRelayCommand(OnOpenSingleFolderAsync);
        OpenMultipleFoldersCommand = new AsyncRelayCommand(OnOpenMultipleFoldersAsync);
    }

    /// <summary>Gets or sets what the last file dialog returned.</summary>
    public string? FileDialogResult { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets what the last folder dialog returned.</summary>
    public string? FolderDialogResult { get; set => SetProperty(ref field, value); }

    /// <summary>Gets the command opening a single-selection file picker.</summary>
    public AsyncRelayCommand OpenSingleFileCommand { get; }

    /// <summary>Gets the command opening a multi-selection file picker.</summary>
    public AsyncRelayCommand OpenMultipleFilesCommand { get; }

    /// <summary>Gets the command opening a file picker filtered to text files.</summary>
    public AsyncRelayCommand OpenTextFilesCommand { get; }

    /// <summary>Gets the command opening a save-file picker.</summary>
    public AsyncRelayCommand SaveFileCommand { get; }

    /// <summary>Gets the command opening a save-file picker with file-type choices.</summary>
    public AsyncRelayCommand SaveFileWithExtensionCommand { get; }

    /// <summary>Gets the command opening a single-selection folder picker.</summary>
    public AsyncRelayCommand OpenSingleFolderCommand { get; }

    /// <summary>Gets the command opening a multi-selection folder picker.</summary>
    public AsyncRelayCommand OpenMultipleFoldersCommand { get; }

    /// <summary>Opens a file picker that accepts one file.</summary>
    private async Task OnOpenSingleFileAsync()
    {
        var files = await _fileDialogService.ShowOpenFileDialogAsync(
            title: "Select a file",
            allowMultiple: false);

        FileDialogResult = Describe(files, "file");
    }

    /// <summary>Opens a file picker that accepts several files.</summary>
    private async Task OnOpenMultipleFilesAsync()
    {
        var files = await _fileDialogService.ShowOpenFileDialogAsync(
            title: "Select multiple files",
            allowMultiple: true);

        FileDialogResult = Describe(files, "file");
    }

    /// <summary>Opens a file picker restricted to text and Markdown files.</summary>
    private async Task OnOpenTextFilesAsync()
    {
        var textFiles = new FilePickerFileType("Text Files")
        {
            Patterns = ["*.txt", "*.md"],
            MimeTypes = ["text/plain", "text/markdown"],
        };

        var files = await _fileDialogService.ShowOpenFileDialogAsync(
            title: "Select text files",
            allowMultiple: true,
            fileTypeFilter: [textFiles, FilePickerFileTypes.All]);

        FileDialogResult = Describe(files, "text file");
    }

    /// <summary>Opens a save-file picker with a suggested name.</summary>
    private async Task OnSaveFileAsync()
    {
        var file = await _fileDialogService.ShowSaveFileDialogAsync(
            title: "Save file",
            suggestedFileName: "document");

        FileDialogResult = file is not null ? $"Save location: {file}" : "Save cancelled";
    }

    /// <summary>Opens a save-file picker offering a choice of file types.</summary>
    private async Task OnSaveFileWithExtensionAsync()
    {
        var textFile = new FilePickerFileType("Text File") { Patterns = ["*.txt"] };
        var markdownFile = new FilePickerFileType("Markdown File") { Patterns = ["*.md"] };

        var file = await _fileDialogService.ShowSaveFileDialogAsync(
            title: "Save document",
            suggestedFileName: "document",
            defaultExtension: "txt",
            fileTypeChoices: [textFile, markdownFile, FilePickerFileTypes.All]);

        FileDialogResult = file is not null ? $"Save location: {file}" : "Save cancelled";
    }

    /// <summary>Opens a folder picker that accepts one folder.</summary>
    private async Task OnOpenSingleFolderAsync()
    {
        var folders = await _folderDialogService.ShowOpenFolderDialogAsync(
            title: "Select a folder",
            allowMultiple: false);

        FolderDialogResult = Describe(folders, "folder");
    }

    /// <summary>Opens a folder picker that accepts several folders.</summary>
    private async Task OnOpenMultipleFoldersAsync()
    {
        var folders = await _folderDialogService.ShowOpenFolderDialogAsync(
            title: "Select multiple folders",
            allowMultiple: true);

        FolderDialogResult = Describe(folders, "folder");
    }

    /// <summary>Renders a picker's result for display.</summary>
    /// <param name="paths">The paths the picker returned; empty when the user cancelled.</param>
    /// <param name="noun">What was being picked, for the empty and singular messages.</param>
    /// <returns>One line per path, or a message saying nothing was selected.</returns>
    private static string Describe(IEnumerable<string> paths, string noun)
    {
        var list = paths.ToList();

        return list.Count switch
        {
            0 => $"No {noun} selected",
            1 => $"Selected: {list[0]}",
            _ => $"Selected {list.Count} {noun}s:\n{string.Join("\n", list)}",
        };
    }
}
