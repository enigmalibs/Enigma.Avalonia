using System.Collections.Generic;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the settings page: rows of <c>SettingsCard</c>, and the application's theme switch.</summary>
/// <remarks>
/// <para>
/// A <c>SettingsCard</c> is either a row that hosts a control — a switch, a button, a combo box — or a
/// row that is itself clickable through <c>Command</c>, never both; the page shows each shape. A
/// <c>SettingsCardExpander</c> is the same row with content that unfolds beneath it.
/// </para>
/// <para>
/// The theme switch is the page's real payload. Assigning
/// <c>Application.Current.RequestedThemeVariant</c> re-resolves every <c>DynamicResource</c> in the
/// window at once, which is why every brush in this application is a dynamic resource and no colour
/// is ever a literal — including in content built in C#. The Charts page listens for the same change
/// and rebuilds its series, so nothing on screen is left behind.
/// </para>
/// </remarks>
public class SettingsPageViewModel : ObservableObject
{
    /// <summary>Initializes a new instance of the <see cref="SettingsPageViewModel"/> class.</summary>
    public SettingsPageViewModel()
    {
        CardClickedCommand = new RelayCommand<string?>(OnCardClicked);
        ConfigureNotificationsCommand = new RelayCommand(OnConfigureNotifications);
    }

    /// <summary>Gets or sets a value indicating whether the application is showing its dark variant.</summary>
    /// <remarks>
    /// The initializer writes the backing field without running the setter, which is what it has to
    /// do: the page is created the first time it is navigated to — possibly long into the session —
    /// so it reads the variant actually in force instead of re-applying one and announcing a switch
    /// that never happened.
    /// </remarks>
    public bool IsDarkTheme
    {
        get;
        set
        {
            if (!SetProperty(ref field, value)) return;

            if (Application.Current is { } app)
                app.RequestedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;

            LastAction = value ? "Switched to the dark theme" : "Switched to the light theme";
        }
    } = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

    /// <summary>Gets the regions offered by the region picker.</summary>
    public IReadOnlyList<string> Regions { get; } =
        ["North America", "Europe", "Asia Pacific", "Latin America"];

    /// <summary>Gets or sets the selected region.</summary>
    public string? SelectedRegion
    {
        get;
        set
        {
            if (SetProperty(ref field, value) && value is not null)
                LastAction = $"Region set to {value}";
        }
    }

    /// <summary>Gets or sets a value indicating whether auto-save is on.</summary>
    public bool IsAutoSaveEnabled
    {
        get;
        set
        {
            if (!SetProperty(ref field, value)) return;

            OnPropertyChanged(nameof(AutoSaveButtonText));
            LastAction = $"Auto-save {(value ? "enabled" : "disabled")}";
        }
    }

    /// <summary>Gets the label on the auto-save toggle button.</summary>
    public string AutoSaveButtonText => IsAutoSaveEnabled ? "Enabled" : "Disabled";

    /// <summary>Gets or sets the description of the last setting the user touched.</summary>
    public string? LastAction { get; set => SetProperty(ref field, value); }

    /// <summary>Gets the command the clickable cards invoke, identified by their parameter.</summary>
    public RelayCommand<string?> CardClickedCommand { get; }

    /// <summary>Gets the command the notifications card's button invokes.</summary>
    public RelayCommand ConfigureNotificationsCommand { get; }

    /// <summary>Records which clickable card was activated.</summary>
    /// <param name="card">The card's <c>CommandParameter</c>.</param>
    private void OnCardClicked(string? card) => LastAction = $"Opened: {card}";

    /// <summary>Records that the notifications button was pressed.</summary>
    private void OnConfigureNotifications() => LastAction = "Configuring notifications...";
}
