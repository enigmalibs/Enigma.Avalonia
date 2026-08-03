using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Enigma.Avalonia.Desktop.Controls.ContentDialog;
using Enigma.Avalonia.Desktop.Services;
using Enigma.Avalonia.Desktop.Showcase.Views;
using Enigma.Icons.Avalonia;
using Enigma.Icons.Phosphor;
using Microsoft.Extensions.DependencyInjection;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the navigation page: programmatic navigation, and vetoing a navigation away.</summary>
/// <remarks>
/// <para>
/// The rail is not the only way to change page. <c>INavigationService.NavigateToAsync</c> takes any
/// Control, and afterwards the service looks for a navigation item whose <c>PageType</c> matches it:
/// navigating to the Settings page selects its footer item, while navigating to the unlisted page
/// clears the selection. Both commands resolve their View and ViewModel from the container, exactly
/// as the shell's own page factory does.
/// </para>
/// <para>
/// The page also implements <see cref="INavigationViewModel"/>. The navigation service awaits
/// <see cref="OnDisappearingAsync"/> before every departure — from the rail or from code — and a
/// <see langword="false"/> return cancels it, restoring the previous rail selection. That is enough
/// to build unsaved-changes protection with no cooperation from the shell.
/// </para>
/// </remarks>
public class NavigationDemoPageViewModel : ObservableObject, INavigationViewModel
{
    /// <summary>The container the two navigation commands resolve their target pages from.</summary>
    private readonly IServiceProvider _services;

    /// <summary>The navigation service.</summary>
    private readonly INavigationService _navigation;

    /// <summary>The content dialog service, used to ask before discarding changes.</summary>
    private readonly IContentDialogService _dialogService;

    /// <summary>The text as it was at the last save; what <see cref="DocumentText"/> is compared against.</summary>
    private string _savedText = string.Empty;

    /// <summary>Initializes a new instance of the <see cref="NavigationDemoPageViewModel"/> class.</summary>
    /// <param name="services">The container the target pages are resolved from.</param>
    /// <param name="navigation">The navigation service.</param>
    /// <param name="dialogService">The content dialog service.</param>
    public NavigationDemoPageViewModel(
        IServiceProvider services,
        INavigationService navigation,
        IContentDialogService dialogService)
    {
        _services = services;
        _navigation = navigation;
        _dialogService = dialogService;

        NavigateToSettingsCommand = new AsyncRelayCommand(OnNavigateToSettingsAsync);
        NavigateToUnlistedCommand = new AsyncRelayCommand(OnNavigateToUnlistedAsync);
        SaveCommand = new RelayCommand(OnSave, CanSaveOrDiscard);
        DiscardCommand = new RelayCommand(OnDiscard, CanSaveOrDiscard);
    }

    /// <summary>Gets the command navigating to the Settings page, which is on the rail's footer.</summary>
    public AsyncRelayCommand NavigateToSettingsCommand { get; }

    /// <summary>Gets the command navigating to a page the rail does not list.</summary>
    public AsyncRelayCommand NavigateToUnlistedCommand { get; }

    /// <summary>Gets the command marking the document saved.</summary>
    public RelayCommand SaveCommand { get; }

    /// <summary>Gets the command restoring the document to its last saved text.</summary>
    public RelayCommand DiscardCommand { get; }

    /// <summary>Gets or sets the document's text.</summary>
    public string DocumentText
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                HasUnsavedChanges = field != _savedText;
        }
    } = string.Empty;

    /// <summary>Gets a value indicating whether the document differs from its last saved text.</summary>
    public bool HasUnsavedChanges
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value)) return;
            SaveCommand.NotifyCanExecuteChanged();
            DiscardCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>Asks whether the page may be navigated away from.</summary>
    /// <returns>
    /// <see langword="true"/> when there is nothing to lose or the user chose to discard;
    /// <see langword="false"/> to cancel the navigation and stay here.
    /// </returns>
    public async Task<bool> OnDisappearingAsync()
    {
        if (!HasUnsavedChanges)
            return true;

        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Unsaved changes";
            dialog.IconData = PhosphorIconSet.Instance
                .GetGlyph(PhosphorIcon.Warning, PhosphorWeight.Regular)
                .ToGeometry();
            dialog[!ContentDialog.IconBrushProperty] = new DynamicResourceExtension("EnigmaWarningBrush");
            dialog.Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    ThemedText(
                        "The document on this page has changes that were never saved.",
                        "EnigmaForegroundBrush"),
                    ThemedText(
                        "Discarding them lets the navigation continue; keeping the editor cancels it "
                        + "and the rail snaps back to this page.",
                        "EnigmaForegroundSecondaryBrush"),
                },
            };
            dialog.PrimaryButtonText = "Discard changes";
            dialog.SecondaryButtonText = "Keep editing";

            // Secondary is the safe answer, so it takes the accent and the initial focus: Enter keeps
            // the work rather than throwing it away.
            dialog.DefaultButton = DefaultButton.Secondary;
        });

        return result == DialogResult.Primary;
    }

    /// <summary>Called after the page appears. Nothing to do here.</summary>
    /// <param name="parameter">The parameter passed to the navigation request; unused.</param>
    /// <returns>A completed task.</returns>
    public Task OnAppearingAsync(object? parameter = null) => Task.CompletedTask;

    /// <summary>Navigates to the Settings page, which the rail lists in its footer.</summary>
    private Task OnNavigateToSettingsAsync() =>
        NavigateToAsync<SettingsPageView, SettingsPageViewModel>();

    /// <summary>Navigates to a page that has no navigation item.</summary>
    private Task OnNavigateToUnlistedAsync() =>
        NavigateToAsync<DummyPageView, DummyPageViewModel>();

    /// <summary>Resolves a page and its ViewModel from the container and navigates to it.</summary>
    /// <typeparam name="TView">The page's View type, registered transient.</typeparam>
    /// <typeparam name="TViewModel">The page's ViewModel type, registered singleton.</typeparam>
    /// <returns>A task completing when the navigation has settled.</returns>
    private Task NavigateToAsync<TView, TViewModel>()
        where TView : Control
        where TViewModel : notnull
    {
        var page = _services.GetRequiredService<TView>();
        page.DataContext = _services.GetRequiredService<TViewModel>();
        return _navigation.NavigateToAsync(page);
    }

    /// <summary>Determines whether the document can be saved or discarded.</summary>
    /// <returns><see langword="true"/> when there are unsaved changes.</returns>
    private bool CanSaveOrDiscard() => HasUnsavedChanges;

    /// <summary>Marks the current text as saved, which lifts the navigation guard.</summary>
    private void OnSave()
    {
        _savedText = DocumentText;
        HasUnsavedChanges = false;
    }

    /// <summary>Restores the last saved text.</summary>
    private void OnDiscard() => DocumentText = _savedText;

    /// <summary>Creates a wrapping text block whose foreground follows a theme brush.</summary>
    /// <param name="text">The text to show.</param>
    /// <param name="brushKey">The theme resource key supplying the foreground.</param>
    /// <returns>The text block, bound to the brush as a dynamic resource.</returns>
    private static TextBlock ThemedText(string text, string brushKey)
    {
        var block = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap };

        // A dynamic resource, not a fixed brush: content built in code follows a runtime theme switch
        // exactly like content declared in XAML.
        block[!TextBlock.ForegroundProperty] = new DynamicResourceExtension(brushKey);
        return block;
    }
}
