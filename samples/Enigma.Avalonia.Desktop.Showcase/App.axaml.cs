using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Enigma.Avalonia.Desktop.Services;
using Enigma.Avalonia.Desktop.Showcase.ViewModels;
using Enigma.Avalonia.Desktop.Showcase.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Enigma.Avalonia.Desktop.Showcase;

/// <summary>The Avalonia application, and the showcase's composition root.</summary>
/// <remarks>
/// The generic host is built and <b>started</b> here, never run: Avalonia's classic desktop lifetime
/// runs the application, and the host only supplies configuration, logging and services.
/// </remarks>
public partial class App : Application
{
    /// <summary>The generic host, stopped and disposed when the desktop lifetime exits.</summary>
    private IHost? _host;

    /// <summary>Loads the application XAML and configures LiveCharts.</summary>
    /// <remarks>
    /// LiveCharts is configured once for the process, before any chart exists — the Charts page relies
    /// on the default mappers and the dark theme being in place by the time it is navigated to.
    /// </remarks>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        LiveCharts.Configure(settings => settings
            .AddSkiaSharp()
            .AddDefaultMappers()
            .AddDarkTheme());
    }

    /// <summary>Builds the host, wires the library's services to the window's host controls, and hands
    /// Avalonia its main window.</summary>
    public override void OnFrameworkInitializationCompleted()
    {
        // A desktop app must not depend on the working directory — it is whatever the shortcut or the
        // shell happened to set — so appsettings.json is read from beside the executable.
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = AppContext.BaseDirectory,
        });

        builder.Services.Configure<ShowcaseOptions>(
            builder.Configuration.GetSection(ShowcaseOptions.SectionName));
        builder.Services.AddEnigmaServices();
        builder.Services.AddPagesAndViewModels();

        var host = builder.Build();
        host.Start();
        _host = host;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = host.Services;
            var mainWindow = services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = services.GetRequiredService<MainWindowViewModel>();

            // All five run before the window is handed over: a service whose host is still
            // unregistered throws the moment a page asks it for a dialog, an overlay or a picker.
            services.GetRequiredService<IContentDialogService>().RegisterHost(mainWindow.HostDialog);
            services.GetRequiredService<IOverlayService>().RegisterHost(mainWindow.HostOverlay);
            services.GetRequiredService<IInfoBarService>().RegisterHost(mainWindow.HostInfoBar);
            services.GetRequiredService<IFileDialogService>().SetStorageProvider(mainWindow.StorageProvider);
            services.GetRequiredService<IFolderDialogService>().SetStorageProvider(mainWindow.StorageProvider);

            desktop.MainWindow = mainWindow;
            desktop.Exit += (_, _) =>
            {
                host.StopAsync().GetAwaiter().GetResult();
                host.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
