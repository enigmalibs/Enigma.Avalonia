using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Enigma.Avalonia.Desktop.Controls.ContentDialog;

/// <summary>
/// A modal dialog overlay that presents a title, content, and up to three action buttons.
/// Use <see cref="ShowAsync"/> to open the dialog and await a <see cref="DialogResult"/>.
/// </summary>
public class ContentDialog : ContentControl
{
    /// <summary>The <c>PART_Overlay</c> border used to detect clicks outside the dialog card, closing it with <see cref="DialogResult.None"/>.</summary>
    private Border? _overlayPart;

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(Title));

    /// <summary>Defines the <see cref="PrimaryButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> PrimaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(PrimaryButtonText));

    /// <summary>Defines the <see cref="SecondaryButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> SecondaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(SecondaryButtonText));

    /// <summary>Defines the <see cref="CloseButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> CloseButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(CloseButtonText));

    /// <summary>Defines the <see cref="PrimaryButtonCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> PrimaryButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(PrimaryButtonCommand));

    /// <summary>Defines the <see cref="SecondaryButtonCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> SecondaryButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(SecondaryButtonCommand));

    /// <summary>Defines the <see cref="CloseButtonCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> CloseButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(CloseButtonCommand));

    /// <summary>Defines the <see cref="IsPrimaryButtonEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsPrimaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsPrimaryButtonEnabled), true);

    /// <summary>Defines the <see cref="IsSecondaryButtonEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsSecondaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsSecondaryButtonEnabled), true);

    /// <summary>Defines the <see cref="IsCloseButtonEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsCloseButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsCloseButtonEnabled), true);

    /// <summary>Defines the <see cref="DefaultButton"/> property.</summary>
    public static readonly StyledProperty<DefaultButton> DefaultButtonProperty =
        AvaloniaProperty.Register<ContentDialog, DefaultButton>(nameof(DefaultButton), DefaultButton.None);

    /// <summary>Defines the <see cref="IsOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsOpen), false);

    /// <summary>Defines the <see cref="OverlayBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> OverlayBrushProperty =
        AvaloniaProperty.Register<ContentDialog, IBrush?>(
            nameof(OverlayBrush),
            new SolidColorBrush(Color.FromArgb(77, 0, 0, 0)));

    /// <summary>Defines the <see cref="DialogResult"/> property.</summary>
    public static readonly StyledProperty<DialogResult> DialogResultProperty =
        AvaloniaProperty.Register<ContentDialog, DialogResult>(nameof(DialogResult), DialogResult.None);

    /// <summary>Defines the <see cref="IconData"/> property.</summary>
    public static readonly StyledProperty<Geometry?> IconDataProperty =
        AvaloniaProperty.Register<ContentDialog, Geometry?>(nameof(IconData));

    /// <summary>Defines the <see cref="IconBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<ContentDialog, IBrush?>(nameof(IconBrush));

    /// <summary>Defines the <see cref="DialogWidth"/> property.</summary>
    public static readonly StyledProperty<double> DialogWidthProperty =
        AvaloniaProperty.Register<ContentDialog, double>(nameof(DialogWidth), double.NaN);

    /// <summary>Defines the <see cref="DialogHeight"/> property.</summary>
    public static readonly StyledProperty<double> DialogHeightProperty =
        AvaloniaProperty.Register<ContentDialog, double>(nameof(DialogHeight), double.NaN);

    /// <summary>Defines the <see cref="DialogMinWidth"/> property.</summary>
    public static readonly StyledProperty<double> DialogMinWidthProperty =
        AvaloniaProperty.Register<ContentDialog, double>(nameof(DialogMinWidth), 320);

    /// <summary>Defines the <see cref="DialogMaxWidth"/> property.</summary>
    public static readonly StyledProperty<double> DialogMaxWidthProperty =
        AvaloniaProperty.Register<ContentDialog, double>(nameof(DialogMaxWidth), 600);

    /// <summary>Defines the <see cref="DialogMinHeight"/> property.</summary>
    public static readonly StyledProperty<double> DialogMinHeightProperty =
        AvaloniaProperty.Register<ContentDialog, double>(nameof(DialogMinHeight), 0);

    /// <summary>Defines the <see cref="DialogMaxHeight"/> property.</summary>
    public static readonly StyledProperty<double> DialogMaxHeightProperty =
        AvaloniaProperty.Register<ContentDialog, double>(nameof(DialogMaxHeight), double.PositiveInfinity);

    /// <summary>Gets or sets the dialog title.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the text label for the primary action button.</summary>
    public string? PrimaryButtonText
    {
        get => GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Gets or sets the text label for the secondary action button.</summary>
    public string? SecondaryButtonText
    {
        get => GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Gets or sets the text label for the close button.</summary>
    public string? CloseButtonText
    {
        get => GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    /// <summary>Gets or sets the command executed when the primary button is clicked.</summary>
    public ICommand? PrimaryButtonCommand
    {
        get => GetValue(PrimaryButtonCommandProperty);
        set => SetValue(PrimaryButtonCommandProperty, value);
    }

    /// <summary>Gets or sets the command executed when the secondary button is clicked.</summary>
    public ICommand? SecondaryButtonCommand
    {
        get => GetValue(SecondaryButtonCommandProperty);
        set => SetValue(SecondaryButtonCommandProperty, value);
    }

    /// <summary>Gets or sets the command executed when the close button is clicked.</summary>
    public ICommand? CloseButtonCommand
    {
        get => GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the primary button is enabled.</summary>
    public bool IsPrimaryButtonEnabled
    {
        get => GetValue(IsPrimaryButtonEnabledProperty);
        set => SetValue(IsPrimaryButtonEnabledProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the secondary button is enabled.</summary>
    public bool IsSecondaryButtonEnabled
    {
        get => GetValue(IsSecondaryButtonEnabledProperty);
        set => SetValue(IsSecondaryButtonEnabledProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the close button is enabled.</summary>
    public bool IsCloseButtonEnabled
    {
        get => GetValue(IsCloseButtonEnabledProperty);
        set => SetValue(IsCloseButtonEnabledProperty, value);
    }

    /// <summary>Gets or sets which button is styled as the default (keyboard-focused) action.</summary>
    public DefaultButton DefaultButton
    {
        get => GetValue(DefaultButtonProperty);
        set => SetValue(DefaultButtonProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the dialog is currently visible.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Gets or sets the brush used to tint the background overlay behind the dialog.</summary>
    public IBrush? OverlayBrush
    {
        get => GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    /// <summary>Gets or sets the result of the last user interaction with the dialog.</summary>
    public DialogResult DialogResult
    {
        get => GetValue(DialogResultProperty);
        set => SetValue(DialogResultProperty, value);
    }

    /// <summary>Gets or sets the icon geometry displayed in the dialog title area.</summary>
    public Geometry? IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    /// <summary>Gets or sets the brush used to paint the dialog icon.</summary>
    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    /// <summary>Gets or sets an explicit width for the dialog card. Defaults to <see cref="double.NaN"/> (auto-size to content, bounded by <see cref="DialogMinWidth"/>/<see cref="DialogMaxWidth"/>).</summary>
    public double DialogWidth
    {
        get => GetValue(DialogWidthProperty);
        set => SetValue(DialogWidthProperty, value);
    }

    /// <summary>Gets or sets an explicit height for the dialog card. Defaults to <see cref="double.NaN"/> (auto-size to content, bounded by <see cref="DialogMinHeight"/>/<see cref="DialogMaxHeight"/>).</summary>
    public double DialogHeight
    {
        get => GetValue(DialogHeightProperty);
        set => SetValue(DialogHeightProperty, value);
    }

    /// <summary>Gets or sets the minimum width of the dialog card. Defaults to 320.</summary>
    public double DialogMinWidth
    {
        get => GetValue(DialogMinWidthProperty);
        set => SetValue(DialogMinWidthProperty, value);
    }

    /// <summary>Gets or sets the maximum width of the dialog card. Defaults to 600; raise it to allow wider content.</summary>
    public double DialogMaxWidth
    {
        get => GetValue(DialogMaxWidthProperty);
        set => SetValue(DialogMaxWidthProperty, value);
    }

    /// <summary>Gets or sets the minimum height of the dialog card. Defaults to 0.</summary>
    public double DialogMinHeight
    {
        get => GetValue(DialogMinHeightProperty);
        set => SetValue(DialogMinHeightProperty, value);
    }

    /// <summary>Gets or sets the maximum height of the dialog card. Defaults to unbounded; set it to make tall content scroll within the card.</summary>
    public double DialogMaxHeight
    {
        get => GetValue(DialogMaxHeightProperty);
        set => SetValue(DialogMaxHeightProperty, value);
    }

    /// <summary>Raised when the dialog is closed, with the <see cref="DialogResult"/> indicating which button was pressed.</summary>
    public event EventHandler<DialogResult>? Closed;

    /// <summary>
    /// Finds template parts for the overlay and all three buttons, and wires click and pointer handlers.
    /// </summary>
    /// <param name="e">The template applied event data.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _overlayPart = e.NameScope.Find<Border>("PART_Overlay");
        if (_overlayPart != null)
        {
            _overlayPart.PointerPressed += OnOverlayPointerPressed;
        }

        var primaryButton = e.NameScope.Find<Button>("PART_PrimaryButton");
        if (primaryButton != null)
        {
            primaryButton.Click += OnPrimaryButtonClick;
        }

        var secondaryButton = e.NameScope.Find<Button>("PART_SecondaryButton");
        if (secondaryButton != null)
        {
            secondaryButton.Click += OnSecondaryButtonClick;
        }

        var closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        if (closeButton != null)
        {
            closeButton.Click += OnCloseButtonClick;
        }
    }

    /// <summary>Closes the dialog with <see cref="DialogResult.None"/> when the Escape key is pressed while the dialog is open.</summary>
    /// <param name="e">The key event data.</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape && IsOpen)
        {
            CloseDialog(DialogResult.None);
            e.Handled = true;
        }
    }

    /// <summary>Closes the dialog with <see cref="DialogResult.None"/> when the overlay background is clicked.</summary>
    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CloseDialog(DialogResult.None);
    }

    /// <summary>Executes <see cref="PrimaryButtonCommand"/> and closes the dialog with <see cref="DialogResult.Primary"/>.</summary>
    private void OnPrimaryButtonClick(object? sender, RoutedEventArgs e)
    {
        PrimaryButtonCommand?.Execute(null);
        CloseDialog(DialogResult.Primary);
    }

    /// <summary>Executes <see cref="SecondaryButtonCommand"/> and closes the dialog with <see cref="DialogResult.Secondary"/>.</summary>
    private void OnSecondaryButtonClick(object? sender, RoutedEventArgs e)
    {
        SecondaryButtonCommand?.Execute(null);
        CloseDialog(DialogResult.Secondary);
    }

    /// <summary>Executes <see cref="CloseButtonCommand"/> and closes the dialog with <see cref="DialogResult.Close"/>.</summary>
    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        CloseButtonCommand?.Execute(null);
        CloseDialog(DialogResult.Close);
    }

    /// <summary>Sets <see cref="DialogResult"/>, closes the dialog, and raises the <see cref="Closed"/> event.</summary>
    /// <param name="result">The result to report.</param>
    private void CloseDialog(DialogResult result)
    {
        DialogResult = result;
        IsOpen = false;
        Closed?.Invoke(this, result);
    }

    /// <summary>Closes the dialog immediately, setting <see cref="DialogResult"/> to <see cref="DialogResult.None"/>.</summary>
    /// <returns>A task that completes when the dialog has closed.</returns>
    public Task HideAsync()
    {
        if (!IsOpen)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource();
        EventHandler<DialogResult>? handler = null;

        handler = (s, result) =>
        {
            Closed -= handler;
            tcs.SetResult();
        };

        Closed += handler;
        CloseDialog(DialogResult.None);

        return tcs.Task;
    }

    /// <summary>Opens the dialog and returns a task that resolves to the <see cref="DialogResult"/> when it closes.</summary>
    /// <returns>A task that completes with the <see cref="DialogResult"/> when the dialog is dismissed.</returns>
    public Task<DialogResult> ShowAsync()
    {
        var tcs = new TaskCompletionSource<DialogResult>();
        EventHandler<DialogResult>? handler = null;

        handler = (s, result) =>
        {
            Closed -= handler;
            tcs.SetResult(result);
        };

        Closed += handler;
        IsOpen = true;

        return tcs.Task;
    }
}
