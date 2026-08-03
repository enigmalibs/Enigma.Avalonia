using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the ribbon page: three tabs of grouped commands over a status readout.</summary>
/// <remarks>
/// <para>
/// Every ribbon control that invokes an action — <c>RibbonButton</c>, <c>RibbonMenuItem</c> inside a
/// <c>RibbonDropDownButton</c>, and <c>RibbonToggleButton</c> — carries <c>Command</c> and
/// <c>CommandParameter</c>, so the fourteen one-shot actions on this page share a single
/// <see cref="RecordActionCommand"/> and identify themselves through the parameter. Only the two
/// toggles get commands of their own, because they also own a piece of state the ribbon binds to.
/// </para>
/// <para>
/// The ribbon itself holds no state beyond which tab is open: <see cref="SelectedTabIndex"/> is bound
/// two-way to <c>Ribbon.SelectedIndex</c>, so switching tabs in the UI and switching them from code
/// are the same operation.
/// </para>
/// </remarks>
public class RibbonTestingPageViewModel : ObservableObject
{
    /// <summary>Initializes a new instance of the <see cref="RibbonTestingPageViewModel"/> class.</summary>
    public RibbonTestingPageViewModel()
    {
        RecordActionCommand = new RelayCommand<string?>(OnRecordAction);
        ToggleBoldCommand = new RelayCommand(OnToggleBold);
        ToggleItalicCommand = new RelayCommand(OnToggleItalic);
    }

    /// <summary>Gets or sets the index of the open ribbon tab.</summary>
    public int SelectedTabIndex { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets the description of the last action a ribbon control invoked.</summary>
    public string StatusText { get; set => SetProperty(ref field, value); } = "Ready";

    /// <summary>Gets or sets a value indicating whether the Bold toggle is on.</summary>
    public bool IsBoldActive { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets a value indicating whether the Italic toggle is on.</summary>
    public bool IsItalicActive { get; set => SetProperty(ref field, value); }

    /// <summary>Gets the command every one-shot ribbon control invokes, identified by its parameter.</summary>
    public RelayCommand<string?> RecordActionCommand { get; }

    /// <summary>Gets the command the Bold toggle invokes after flipping <see cref="IsBoldActive"/>.</summary>
    public RelayCommand ToggleBoldCommand { get; }

    /// <summary>Gets the command the Italic toggle invokes after flipping <see cref="IsItalicActive"/>.</summary>
    public RelayCommand ToggleItalicCommand { get; }

    /// <summary>Records which ribbon control was invoked.</summary>
    /// <param name="action">The invoking control's <c>CommandParameter</c>.</param>
    private void OnRecordAction(string? action) => StatusText = action ?? "Unnamed action";

    /// <summary>Records the Bold toggle's new state.</summary>
    /// <remarks>
    /// <c>IsChecked</c> is bound two-way and has already been written by the time the command runs, so
    /// the handler reads the property rather than flipping it — flipping here would undo the binding.
    /// </remarks>
    private void OnToggleBold() => StatusText = IsBoldActive ? "Bold enabled" : "Bold disabled";

    /// <summary>Records the Italic toggle's new state.</summary>
    private void OnToggleItalic() => StatusText = IsItalicActive ? "Italic enabled" : "Italic disabled";
}
