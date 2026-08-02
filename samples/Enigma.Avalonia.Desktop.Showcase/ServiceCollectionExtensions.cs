using Enigma.Avalonia.Desktop.Services;
using Enigma.Avalonia.Desktop.Showcase.ViewModels;
using Enigma.Avalonia.Desktop.Showcase.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Enigma.Avalonia.Desktop.Showcase;

/// <summary>Registers the showcase's services, pages and ViewModels.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Registers the library's six services.</summary>
        /// <remarks>
        /// All six are singletons because all six hold process-wide state: three of them own a host
        /// control registered once at startup, two own the window's storage provider, and the
        /// navigation service owns the rail's items and the current page. This method is the whole
        /// integration surface — a consumer application copies it verbatim.
        /// </remarks>
        public void AddEnigmaServices()
        {
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IContentDialogService, ContentDialogService>();
            services.AddSingleton<IOverlayService, OverlayService>();
            services.AddSingleton<IInfoBarService, InfoBarService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IFolderDialogService, FolderDialogService>();
        }

        /// <summary>Registers the main window and every page View and ViewModel.</summary>
        /// <remarks>
        /// Views are transient and ViewModels are singletons, deliberately: navigating back to a page
        /// builds a fresh Control but re-attaches the ViewModel it had before, so a page's state
        /// survives leaving it while its visual tree does not leak.
        /// </remarks>
        public void AddPagesAndViewModels()
        {
            services.AddSingleton<MainWindow>();

            services.AddTransient<HomePageView>();
            services.AddTransient<BaseControlsPageView>();
            services.AddTransient<EditorsTestingPageView>();
            services.AddTransient<DialogsTestingPageView>();
            services.AddTransient<ServicesTestingPageView>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<HomePageViewModel>();
            services.AddSingleton<BaseControlsPageViewModel>();
            services.AddSingleton<EditorsTestingPageViewModel>();
            services.AddSingleton<DialogsTestingPageViewModel>();
            services.AddSingleton<ServicesTestingPageViewModel>();
        }
    }
}
