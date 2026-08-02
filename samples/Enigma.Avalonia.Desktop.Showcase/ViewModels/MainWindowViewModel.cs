using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.ComponentModel;
using Enigma.Avalonia.Desktop.Controls.Navigation;
using Enigma.Avalonia.Desktop.Services;
using Enigma.Avalonia.Desktop.Showcase.Views;
using Enigma.Icons.Avalonia;
using Enigma.Icons.Phosphor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the main window: the navigation rail, the four hosted services, and the logo.</summary>
/// <remarks>
/// The window's own ViewModel owns the page catalogue because the navigation rail is the shell, not a
/// page. Each entry carries the View type and the ViewModel type; <see cref="CreatePage"/> resolves
/// both from the container when the rail selection changes, so a page is never constructed here.
/// </remarks>
public class MainWindowViewModel : ObservableObject
{
    /// <summary>The container the page factory resolves Views and ViewModels from.</summary>
    private readonly IServiceProvider _services;

    /// <summary>
    /// The page catalogue by configuration key. <see cref="NavigationItem"/> has no key of its own —
    /// it is a control — so the showcase keeps this map to resolve
    /// <see cref="ShowcaseOptions.InitialPage"/> to an item.
    /// </summary>
    private readonly Dictionary<string, NavigationItem> _pagesByKey = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Initializes a new instance of the <see cref="MainWindowViewModel"/> class.</summary>
    /// <param name="services">The container pages are resolved from.</param>
    /// <param name="navigation">The navigation service driving the rail and the content area.</param>
    /// <param name="dialogService">The content dialog service, used by the window's close confirmation.</param>
    /// <param name="overlayService">The overlay service, exposed for the pages that demonstrate it.</param>
    /// <param name="infoBarService">The info bar service, exposed for the pages that demonstrate it.</param>
    /// <param name="options">The showcase settings bound from <c>appsettings.json</c>.</param>
    public MainWindowViewModel(
        IServiceProvider services,
        INavigationService navigation,
        IContentDialogService dialogService,
        IOverlayService overlayService,
        IInfoBarService infoBarService,
        IOptions<ShowcaseOptions> options)
    {
        _services = services;
        Navigation = navigation;
        DialogService = dialogService;
        OverlayService = overlayService;
        InfoBarService = infoBarService;

        var settings = options.Value;
        Title = settings.Title;
        Logo = CreateLogo();

        Navigation.PageFactory = CreatePage;

        AddPage("home", "Home", PhosphorIcon.House, typeof(HomePageView), typeof(HomePageViewModel));

        NavigateToInitialPage(settings.InitialPage);
    }

    /// <summary>Gets the navigation service the rail and the content area are bound to.</summary>
    public INavigationService Navigation { get; }

    /// <summary>Gets the content dialog service.</summary>
    public IContentDialogService DialogService { get; }

    /// <summary>Gets the overlay service.</summary>
    public IOverlayService OverlayService { get; }

    /// <summary>Gets the info bar service.</summary>
    public IInfoBarService InfoBarService { get; }

    /// <summary>Gets the window title, from <see cref="ShowcaseOptions.Title"/>.</summary>
    public string Title { get; }

    /// <summary>Gets the mark shown at the top of the navigation rail.</summary>
    public object Logo { get; }

    /// <summary>Adds a page to the navigation rail and to the key map.</summary>
    /// <param name="key">The <c>appsettings.json</c> key for the page.</param>
    /// <param name="header">The label shown in the rail.</param>
    /// <param name="icon">The Phosphor glyph shown in the rail.</param>
    /// <param name="pageType">The page's View type, resolved as a transient.</param>
    /// <param name="viewModelType">The page's ViewModel type, resolved as a singleton.</param>
    /// <param name="footer">Whether the item belongs to the rail's footer rather than its main list.</param>
    private void AddPage(
        string key,
        string header,
        PhosphorIcon icon,
        Type pageType,
        Type viewModelType,
        bool footer = false)
    {
        var item = new NavigationItem
        {
            Header = header,
            IconData = PhosphorIconSet.Instance.GetGlyph(icon, PhosphorWeight.Regular).ToGeometry(),
            PageType = pageType,
            PageViewModelType = viewModelType,
        };

        (footer ? Navigation.FooterItems : Navigation.Items).Add(item);
        _pagesByKey[key] = item;
    }

    /// <summary>Resolves a navigation item's View and ViewModel from the container.</summary>
    /// <param name="item">The item being navigated to.</param>
    /// <returns>The page Control, with its DataContext set.</returns>
    /// <exception cref="InvalidOperationException">The registered page type is not a Control.</exception>
    private Control CreatePage(NavigationItem item)
    {
        if (_services.GetRequiredService(item.PageType) is not Control page)
            throw new InvalidOperationException($"Page type {item.PageType} is not a Control.");

        page.DataContext = _services.GetRequiredService(item.PageViewModelType);
        return page;
    }

    /// <summary>Opens the configured startup page, falling back to the first item in the rail.</summary>
    /// <param name="key">The configured page key.</param>
    private void NavigateToInitialPage(string key)
    {
        if (!_pagesByKey.TryGetValue(key, out var item) && Navigation.Items.Count > 0)
            item = Navigation.Items[0];

        // Assigning SelectedItem runs the page factory and highlights the rail entry in one step; it
        // completes synchronously here because nothing is on screen yet to veto the navigation.
        if (item is not null)
            Navigation.SelectedItem = item;
    }

    /// <summary>Builds the navigation rail's logo.</summary>
    /// <returns>An icon bound to the theme's accent brush.</returns>
    private static Control CreateLogo()
    {
        var logo = new Icon
        {
            Kind = PhosphorIcon.Cube,
            Weight = PhosphorWeight.Duotone,
            Size = 28,
        };

        // A dynamic resource, not a fixed brush: the mark follows a runtime theme switch like every
        // other themed surface in the window.
        logo[!Icon.ForegroundProperty] = new DynamicResourceExtension("EnigmaAccentBrush");
        return logo;
    }
}
