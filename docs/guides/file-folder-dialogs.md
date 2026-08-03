# File and folder dialogs

`Enigma.Avalonia.Desktop` exposes the platform's file and folder pickers as two injectable services,
`IFileDialogService` and `IFolderDialogService`. Both wrap Avalonia's `IStorageProvider`, which hangs
off a `TopLevel` and is therefore normally reachable only from a window — with these services a
ViewModel takes the interface from the container and never touches a `Window`.

The trade is one wiring step: the provider is handed to each service once, at startup, through
`SetStorageProvider`. Until that happens the services have nothing to show a dialog with, so every
dialog method — the extension methods included — throws
`InvalidOperationException("Storage provider is not set")` before doing any work. The two services are
independent; each needs its own call.

Everything is asynchronous and the awaited value *is* the user's decision: no callback, no event, and
cancellation is not an exception — a cancelled open dialog returns an empty list, a cancelled save
dialog returns `null`.

## Operations

| Operation | Member | Returns |
|-----------|--------|---------|
| Register the provider | `IFileDialogService.SetStorageProvider(IStorageProvider storageProvider)` | `void` — call once at startup. |
| Inspect the provider | `IFileDialogService.StorageProvider` | `IStorageProvider?` — `null` until registered. |
| Pick files to open | `IFileDialogService.ShowOpenFileDialogAsync(FilePickerOpenOptions options)` | `Task<IReadOnlyList<IStorageFile>>` — empty when cancelled. |
| Pick a save location | `IFileDialogService.ShowSaveFileDialogAsync(FilePickerSaveOptions options)` | `Task<IStorageFile?>` — `null` when cancelled. |
| Register the provider | `IFolderDialogService.SetStorageProvider(IStorageProvider storageProvider)` | `void` — separate from the file service's. |
| Inspect the provider | `IFolderDialogService.StorageProvider` | `IStorageProvider?` — `null` until registered. |
| Pick folders | `IFolderDialogService.ShowOpenFolderDialogAsync(FolderPickerOpenOptions options)` | `Task<IReadOnlyList<IStorageFolder>>` — empty when cancelled. |

Those seven members are the whole contract. `IStorageFile` and `IStorageFolder` are handles, not
paths: they carry a `Uri Path`, a `Name` and stream access, and may have no local path at all.

## Convenience extension methods

`FileDialogServiceExtensions` and `FolderDialogServiceExtensions` add same-named overloads that build
the options object for you and project the result down to local path strings. They are C# extension
methods, so `using Enigma.Avalonia.Desktop.Services;` must be in scope — the same using that brings in
the interfaces.

| Extension (on) | Parameters, with defaults | Returns |
|----------------|---------------------------|---------|
| `ShowOpenFileDialogAsync` (`IFileDialogService`) | `string? title = null`, `bool allowMultiple = false`, `string? suggestedStartLocation = null`, `string? suggestedFileName = null`, `IReadOnlyList<FilePickerFileType>? fileTypeFilter = null` | `Task<IEnumerable<string>>` |
| `ShowSaveFileDialogAsync` (`IFileDialogService`) | `string? title = null`, `string? suggestedStartLocation = null`, `string? suggestedFileName = null`, `string? defaultExtension = null`, `bool showOverwritePrompt = true`, `IReadOnlyList<FilePickerFileType>? fileTypeChoices = null` | `Task<string?>` |
| `ShowOpenFolderDialogAsync` (`IFolderDialogService`) | `string? title = null`, `bool allowMultiple = false`, `string? suggestedStartLocation = null`, `string? suggestedFileName = null` | `Task<IEnumerable<string>>` |

Each parameter sets one option property and is skipped when left `null`: `title` → `Title`,
`allowMultiple` → `AllowMultiple`, `suggestedFileName` → `SuggestedFileName` (the folder overload uses
it for the folder name), `defaultExtension` → `DefaultExtension`, `showOverwritePrompt` →
`ShowOverwritePrompt`. `suggestedStartLocation` is a *path string*, resolved through
`IStorageProvider.TryGetFolderFromPathAsync` into the `IStorageFolder` the option actually wants.

The filter parameters pass through unchanged: `fileTypeFilter` becomes
`FilePickerOpenOptions.FileTypeFilter` (the types an open dialog shows) and `fileTypeChoices` becomes
`FilePickerSaveOptions.FileTypeChoices` (the save dialog's type dropdown). Build entries with
`FilePickerFileType` — `Patterns` for globs, `MimeTypes` for Linux and browsers,
`AppleUniformTypeIdentifiers` for macOS and iOS — or take a ready-made one from `FilePickerFileTypes`
(`All`, `TextPlain`, `ImageAll`, `ImagePng`, `ImageJpg`, `ImageWebp`). Overload resolution follows the
argument: an options object selects the interface method and yields handles; named simple arguments —
or none at all — select the extension and yield `string` paths.

## Key types

| Type | Namespace | Role |
|------|-----------|------|
| `IFileDialogService` | `Enigma.Avalonia.Desktop.Services` | File open/save picker contract. Inject this. |
| `FileDialogService` | `Enigma.Avalonia.Desktop.Services` | Default implementation. Register as a singleton. |
| `IFolderDialogService` | `Enigma.Avalonia.Desktop.Services` | Folder picker contract. Inject this. |
| `FolderDialogService` | `Enigma.Avalonia.Desktop.Services` | Default implementation. Register as a singleton. |
| `FileDialogServiceExtensions`, `FolderDialogServiceExtensions` | `Enigma.Avalonia.Desktop.Services` | The path-string overloads of each service. |
| `IStorageProvider` | `Avalonia.Platform.Storage` | The platform picker, from `TopLevel.StorageProvider`. |
| `FilePickerOpenOptions`, `FilePickerSaveOptions`, `FolderPickerOpenOptions` | `Avalonia.Platform.Storage` | Full option objects for the interface methods. |
| `IStorageFile`, `IStorageFolder` | `Avalonia.Platform.Storage` | Selection handles: `Path`, `Name`, stream access. |
| `FilePickerFileType`, `FilePickerFileTypes` | `Avalonia.Platform.Storage` | A file-type filter entry, and the built-in ones. |

## Usage

### Wiring the storage provider at startup

`StorageProvider` is a `TopLevel` member, so the earliest safe moment is when the main window exists —
do it before the window is shown, or a page that picks a file on load will throw. From a control
rather than a window the equivalent source is `TopLevel.GetTopLevel(control)?.StorageProvider`.

```csharp
using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Enigma.Avalonia.Desktop.Services;
using MyApp.Views;

namespace MyApp;

public partial class App : Application
{
    // However the instances are obtained — 'new' here, resolved from a container in a DI app — each
    // service is handed the provider exactly once.
    private readonly IFileDialogService _fileDialogService = new FileDialogService();
    private readonly IFolderDialogService _folderDialogService = new FolderDialogService();

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow mainWindow = new();
            _fileDialogService.SetStorageProvider(mainWindow.StorageProvider);
            _folderDialogService.SetStorageProvider(mainWindow.StorageProvider);
            Console.WriteLine($"Pickers ready: {_fileDialogService.StorageProvider is not null}");
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

### Opening files from a ViewModel

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Enigma.Avalonia.Desktop.Services;

namespace MyApp.ViewModels;

public class OpenFilesViewModel : ObservableObject
{
    private readonly IFileDialogService _fileDialogService;

    public OpenFilesViewModel(IFileDialogService fileDialogService)
    {
        _fileDialogService = fileDialogService;
        OpenFilesCommand = new AsyncRelayCommand(OnOpenFilesAsync);
    }

    public string? Status { get; set => SetProperty(ref field, value); }

    public IAsyncRelayCommand OpenFilesCommand { get; }

    private async Task OnOpenFilesAsync()
    {
        IEnumerable<string> paths = await _fileDialogService.ShowOpenFileDialogAsync(
            title: "Select files",
            allowMultiple: true);

        // An empty sequence is the cancel signal; nothing throws for it.
        string joined = string.Join(Environment.NewLine, paths);
        Status = joined.Length == 0 ? "Cancelled" : joined;
    }
}
```

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.OpenFilesView"
             x:DataType="vm:OpenFilesViewModel">
  <StackPanel Margin="24" Spacing="12">
    <Button Content="Select files" Command="{Binding OpenFilesCommand}" />
    <TextBlock Text="{Binding Status}" TextWrapping="Wrap" />
  </StackPanel>
</UserControl>
```

### Filtering by file type, and saving

One `FilePickerFileType` serves both directions: as the open dialog's filter and as an entry in the
save dialog's type dropdown.

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Enigma.Avalonia.Desktop.Services;

namespace MyApp.Documents;

public sealed class TextDocumentDialogs(IFileDialogService fileDialogService)
{
    private static readonly FilePickerFileType TextFiles = new("Text files")
    {
        Patterns = ["*.txt", "*.md"],
        MimeTypes = ["text/plain", "text/markdown"],
        AppleUniformTypeIdentifiers = ["public.plain-text"],
    };

    // The first entry is the initially selected filter; FilePickerFileTypes.All lets the user out of it.
    public async Task<string?> PickAsync()
    {
        IEnumerable<string> paths = await fileDialogService.ShowOpenFileDialogAsync(
            title: "Select a text file",
            suggestedStartLocation: Environment.CurrentDirectory,
            fileTypeFilter: [TextFiles, FilePickerFileTypes.All]);

        return paths.FirstOrDefault();
    }

    // defaultExtension is appended when the user types a bare name; showOverwritePrompt is already
    // true by default and is spelled out here only to show where it belongs.
    public async Task<bool> SaveAsync(string content)
    {
        string? path = await fileDialogService.ShowSaveFileDialogAsync(
            title: "Save document",
            suggestedFileName: "document",
            defaultExtension: "txt",
            showOverwritePrompt: true,
            fileTypeChoices: [TextFiles]);

        if (path is null)
            return false;

        await File.WriteAllTextAsync(path, content);
        return true;
    }
}
```

### Picking folders

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enigma.Avalonia.Desktop.Services;

namespace MyApp.Documents;

public sealed class WorkspacePicker(IFolderDialogService folderDialogService)
{
    public async Task<IReadOnlyList<string>> PickWorkspacesAsync()
    {
        IEnumerable<string> paths = await folderDialogService.ShowOpenFolderDialogAsync(
            title: "Select workspace folders",
            allowMultiple: true,
            suggestedStartLocation: Environment.CurrentDirectory,
            suggestedFileName: "Projects");

        return paths.ToList();
    }
}
```

### Working with storage handles instead of paths

Pass the options object when you need the `IStorageFile` itself — to read or write through its stream,
or because the platform gives the selection no local path. This is also where the missing-provider
failure shows: an `InvalidOperationException` here means `SetStorageProvider` was never called, a
wiring bug rather than anything the user did.

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Enigma.Avalonia.Desktop.Services;

namespace MyApp.Documents;

public sealed class DocumentLoader(IFileDialogService fileDialogService)
{
    public async Task<string?> LoadAsync()
    {
        IReadOnlyList<IStorageFile> files;

        try
        {
            files = await fileDialogService.ShowOpenFileDialogAsync(new FilePickerOpenOptions
            {
                Title = "Open a document",
                FileTypeFilter = [FilePickerFileTypes.TextPlain],
            });
        }
        catch (InvalidOperationException exception)
        {
            Console.WriteLine(exception.Message); // "Storage provider is not set"
            return null;
        }

        if (files.Count == 0)
            return null;

        await using Stream stream = await files[0].OpenReadAsync();
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }
}
```

## Notes

- The extension overloads project the selection with `IStorageItem.TryGetLocalPath()` and drop every
  item that has none, so the returned sequence can be shorter than what the user picked. When that
  matters, use the options overloads and keep the handles.
- `suggestedStartLocation` is resolved with `TryGetFolderFromPathAsync`; a path that does not exist
  resolves to `null` and the dialog opens at the platform default. It is never an error.
- `SetStorageProvider` validates nothing and can be called again — the last provider wins, which is
  what a replacement main window needs. `StorageProvider` is readable on both interfaces, so a caller
  can check readiness (`is not null`) and probe `CanOpen`, `CanSave` or `CanPickFolder` before
  offering the action.
- Dialogs are modal to the window that owns the provider. Call them from the UI thread; awaiting the
  task keeps the caller on it.
