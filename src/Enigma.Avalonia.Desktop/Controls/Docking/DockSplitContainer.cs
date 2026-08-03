using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Enigma.Avalonia.Desktop.Controls.Docking;

/// <summary>
/// A templated control that arranges two child controls side-by-side or stacked,
/// separated by a resizable <see cref="GridSplitter"/>.
/// </summary>
public class DockSplitContainer : TemplatedControl
{
    /// <summary>The <c>PART_Grid</c> template part whose column or row definitions are reconfigured on orientation change.</summary>
    private Grid? _grid;

    /// <summary>The <c>PART_First</c> ContentControl that hosts the first child.</summary>
    private ContentControl? _first;

    /// <summary>The <c>PART_Splitter</c> GridSplitter that allows the user to resize the two children.</summary>
    private GridSplitter? _splitter;

    /// <summary>The <c>PART_Second</c> ContentControl that hosts the second child.</summary>
    private ContentControl? _second;

    /// <summary>Defines the <see cref="First"/> property.</summary>
    public static readonly StyledProperty<Control?> FirstProperty =
        AvaloniaProperty.Register<DockSplitContainer, Control?>(nameof(First));

    /// <summary>Defines the <see cref="Second"/> property.</summary>
    public static readonly StyledProperty<Control?> SecondProperty =
        AvaloniaProperty.Register<DockSplitContainer, Control?>(nameof(Second));

    /// <summary>Defines the <see cref="Orientation"/> property.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<DockSplitContainer, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <summary>Defines the <see cref="FirstSize"/> property.</summary>
    public static readonly StyledProperty<GridLength> FirstSizeProperty =
        AvaloniaProperty.Register<DockSplitContainer, GridLength>(nameof(FirstSize), new GridLength(1, GridUnitType.Star));

    /// <summary>Defines the <see cref="SecondSize"/> property.</summary>
    public static readonly StyledProperty<GridLength> SecondSizeProperty =
        AvaloniaProperty.Register<DockSplitContainer, GridLength>(nameof(SecondSize), new GridLength(1, GridUnitType.Star));

    /// <summary>Gets or sets the first (left or top) child control.</summary>
    public Control? First
    {
        get => GetValue(FirstProperty);
        set => SetValue(FirstProperty, value);
    }

    /// <summary>Gets or sets the second (right or bottom) child control.</summary>
    public Control? Second
    {
        get => GetValue(SecondProperty);
        set => SetValue(SecondProperty, value);
    }

    /// <summary>Gets or sets the axis along which the two children are arranged.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>Gets or sets the initial grid size allocated to the first child.</summary>
    public GridLength FirstSize
    {
        get => GetValue(FirstSizeProperty);
        set => SetValue(FirstSizeProperty, value);
    }

    /// <summary>Gets or sets the initial grid size allocated to the second child.</summary>
    public GridLength SecondSize
    {
        get => GetValue(SecondSizeProperty);
        set => SetValue(SecondSizeProperty, value);
    }

    /// <summary>
    /// Finds template parts <c>PART_Grid</c>, <c>PART_First</c>, <c>PART_Splitter</c>, and <c>PART_Second</c>,
    /// then configures layout and pseudo-classes.
    /// </summary>
    /// <param name="e">The template applied event data.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _grid = e.NameScope.Find<Grid>("PART_Grid");
        _first = e.NameScope.Find<ContentControl>("PART_First");
        _splitter = e.NameScope.Find<GridSplitter>("PART_Splitter");
        _second = e.NameScope.Find<ContentControl>("PART_Second");

        ConfigureLayout();
        UpdatePseudoClasses();
    }

    /// <summary>
    /// Reconfigures the grid layout when <see cref="Orientation"/>, <see cref="FirstSize"/>,
    /// or <see cref="SecondSize"/> changes.
    /// </summary>
    /// <param name="change">Details about the property that changed.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty)
        {
            ConfigureLayout();
            UpdatePseudoClasses();
        }
        else if (change.Property == FirstSizeProperty || change.Property == SecondSizeProperty)
        {
            ConfigureLayout();
        }
    }

    /// <summary>
    /// Rebuilds the grid's column or row definitions and repositions all template parts
    /// based on the current <see cref="Orientation"/>, <see cref="FirstSize"/>, and <see cref="SecondSize"/>.
    /// </summary>
    private void ConfigureLayout()
    {
        if (_grid == null || _first == null || _splitter == null || _second == null)
            return;

        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();

        if (Orientation == Orientation.Horizontal)
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition(FirstSize));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(SecondSize));

            Grid.SetColumn(_first, 0);
            Grid.SetRow(_first, 0);
            Grid.SetColumn(_splitter, 1);
            Grid.SetRow(_splitter, 0);
            Grid.SetColumn(_second, 2);
            Grid.SetRow(_second, 0);

            // Reset rows
            Grid.SetRowSpan(_first, 1);
            Grid.SetRowSpan(_splitter, 1);
            Grid.SetRowSpan(_second, 1);
            Grid.SetColumnSpan(_first, 1);
            Grid.SetColumnSpan(_splitter, 1);
            Grid.SetColumnSpan(_second, 1);

            _splitter.ResizeDirection = GridResizeDirection.Columns;
        }
        else
        {
            _grid.RowDefinitions.Add(new RowDefinition(FirstSize));
            _grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _grid.RowDefinitions.Add(new RowDefinition(SecondSize));

            Grid.SetRow(_first, 0);
            Grid.SetColumn(_first, 0);
            Grid.SetRow(_splitter, 1);
            Grid.SetColumn(_splitter, 0);
            Grid.SetRow(_second, 2);
            Grid.SetColumn(_second, 0);

            // Reset spans
            Grid.SetRowSpan(_first, 1);
            Grid.SetRowSpan(_splitter, 1);
            Grid.SetRowSpan(_second, 1);
            Grid.SetColumnSpan(_first, 1);
            Grid.SetColumnSpan(_splitter, 1);
            Grid.SetColumnSpan(_second, 1);

            _splitter.ResizeDirection = GridResizeDirection.Rows;
        }
    }

    /// <summary>Sets the <c>:horizontal</c> and <c>:vertical</c> pseudo-classes to reflect the current <see cref="Orientation"/>.</summary>
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":horizontal", Orientation == Orientation.Horizontal);
        PseudoClasses.Set(":vertical", Orientation == Orientation.Vertical);
    }
}
