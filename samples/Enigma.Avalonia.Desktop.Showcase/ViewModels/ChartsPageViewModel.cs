using System;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the charts page, and keeps LiveCharts in step with the application's theme.</summary>
/// <remarks>
/// <para>
/// LiveCharts is not an Avalonia-themed control: its axes, labels and series colours come from a
/// process-wide configuration, not from resolved <c>DynamicResource</c> brushes, so nothing here
/// follows a theme switch on its own. This ViewModel is the bridge — it reconfigures LiveCharts for
/// the new variant and rebuilds the series, which is what makes the charts change with everything
/// else on screen when the Settings page flips the theme.
/// </para>
/// <para>
/// The series have to be rebuilt rather than merely re-styled: the theme supplies each series' paints
/// at construction. That is also why the configuration runs in the constructor before the first
/// build, and not only on the event — this ViewModel is a singleton created the first time the page
/// is navigated to, which may be long after the theme was last changed.
/// </para>
/// <para>
/// The subscription is never removed. A page ViewModel here lives as long as the application does, so
/// there is nothing to unsubscribe from and no leak to avoid; a ViewModel with a shorter life would
/// need to detach.
/// </para>
/// </remarks>
public class ChartsPageViewModel : ObservableObject
{
    /// <summary>Initializes a new instance of the <see cref="ChartsPageViewModel"/> class.</summary>
    public ChartsPageViewModel()
    {
        ConfigureLiveChartsTheme();
        BuildSeries();

        if (Application.Current is { } app)
            app.ActualThemeVariantChanged += OnThemeChanged;
    }

    /// <summary>Gets or sets the two-series line chart.</summary>
    public ISeries[] LineSeries { get; set => SetProperty(ref field, value); } = [];

    /// <summary>Gets or sets the two-series column chart.</summary>
    public ISeries[] ColumnSeries { get; set => SetProperty(ref field, value); } = [];

    /// <summary>Gets or sets the pie chart's slices, one series each.</summary>
    public ISeries[] PieSeries { get; set => SetProperty(ref field, value); } = [];

    /// <summary>Gets or sets the line chart's day-of-week category axis.</summary>
    public Axis[] LineXAxes { get; set => SetProperty(ref field, value); } = [];

    /// <summary>Gets or sets the column chart's category axis.</summary>
    public Axis[] ColumnXAxes { get; set => SetProperty(ref field, value); } = [];

    /// <summary>Gets or sets the column chart's value axis.</summary>
    public Axis[] ColumnYAxes { get; set => SetProperty(ref field, value); } = [];

    /// <summary>Reconfigures LiveCharts and rebuilds every series for the new theme variant.</summary>
    /// <param name="sender">The application whose theme changed.</param>
    /// <param name="e">Empty event data.</param>
    private void OnThemeChanged(object? sender, EventArgs e)
    {
        ConfigureLiveChartsTheme();
        BuildSeries();
    }

    /// <summary>Points LiveCharts at the theme matching the application's current variant.</summary>
    private static void ConfigureLiveChartsTheme()
    {
        var isDark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

        LiveCharts.Configure(settings => settings
            .AddSkiaSharp()
            .AddDefaultMappers());

        if (isDark)
            LiveCharts.Configure(settings => settings.AddDarkTheme());
        else
            LiveCharts.Configure(settings => settings.AddLightTheme());
    }

    /// <summary>Builds every series and axis, taking its paints from the configuration in force.</summary>
    private void BuildSeries()
    {
        LineSeries =
        [
            new LineSeries<double> { Name = "Downloads", Values = [2, 5, 4, 8, 6, 12, 9] },
            new LineSeries<double> { Name = "Revenue", Values = [1, 3, 5, 3, 7, 10, 13] },
        ];

        LineXAxes = [new Axis { Labels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"] }];

        ColumnSeries =
        [
            new ColumnSeries<double> { Name = "2024", Values = [120, 95, 140, 110, 160] },
            new ColumnSeries<double> { Name = "2025", Values = [150, 120, 170, 130, 190] },
        ];

        ColumnXAxes = [new Axis { Labels = ["Mon", "Tue", "Wed", "Thu", "Fri"] }];
        ColumnYAxes = [new Axis { Name = "Sales" }];

        PieSeries =
        [
            new PieSeries<double> { Name = "Chrome", Values = [65] },
            new PieSeries<double> { Name = "Firefox", Values = [12] },
            new PieSeries<double> { Name = "Safari", Values = [10] },
            new PieSeries<double> { Name = "Edge", Values = [8] },
            new PieSeries<double> { Name = "Other", Values = [5] },
        ];
    }
}
