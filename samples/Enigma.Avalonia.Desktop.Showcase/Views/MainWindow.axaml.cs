using Avalonia.Controls;
using Enigma.Avalonia.Desktop.Controls.ContentDialog;
using Enigma.Avalonia.Desktop.Showcase.ViewModels;

namespace Enigma.Avalonia.Desktop.Showcase.Views;

/// <summary>The showcase's main window: the navigation shell plus the dialog, overlay and info bar
/// hosts the library's services drive.</summary>
public partial class MainWindow : Window
{
    /// <summary>Set once the user has confirmed the close, so the second Close() goes through.</summary>
    private bool _forceClose;

    /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    /// <summary>Vetoes the close, asks for confirmation through the dialog service, and closes for
    /// real if the user agrees.</summary>
    /// <param name="sender">The window.</param>
    /// <param name="e">The closing event data, cancelled on the first pass.</param>
    /// <remarks>
    /// <c>async void</c> is right here and almost nowhere else: an event handler cannot return a
    /// Task, so the close is vetoed synchronously and re-issued once the dialog has answered.
    /// </remarks>
    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_forceClose)
            return;

        e.Cancel = true;

        if (DataContext is not MainWindowViewModel vm)
            return;

        var result = await vm.DialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Quit the showcase";
            dialog.Content = "Are you sure you want to leave?";
            dialog.PrimaryButtonText = "Leave";
            dialog.CloseButtonText = "Cancel";
        });

        if (result != DialogResult.Primary)
            return;

        _forceClose = true;
        Close();
    }
}
