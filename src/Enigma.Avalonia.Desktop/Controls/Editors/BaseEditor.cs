using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;

namespace Enigma.Avalonia.Desktop.Controls.Editors;

/// <summary>
/// Base class for all Enigma editor controls. Extends <see cref="TextBox"/> with
/// title, unit, leading/action content areas, validation error display, and
/// configurable select-all-on-focus behaviour.
/// </summary>
public class BaseEditor : TextBox
{
    /// <summary>Identifies the <see cref="Title"/> styled property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<BaseEditor, string?>(nameof(Title));

    /// <summary>Identifies the <see cref="Unit"/> styled property.</summary>
    public static readonly StyledProperty<string?> UnitProperty =
        AvaloniaProperty.Register<BaseEditor, string?>(nameof(Unit));

    /// <summary>Identifies the <see cref="LeadingContent"/> styled property.</summary>
    public static readonly StyledProperty<object?> LeadingContentProperty =
        AvaloniaProperty.Register<BaseEditor, object?>(nameof(LeadingContent));

    /// <summary>Identifies the <see cref="ActionContent"/> styled property.</summary>
    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<BaseEditor, object?>(nameof(ActionContent));

    /// <summary>Identifies the <see cref="HasValidationError"/> styled property.</summary>
    public static readonly StyledProperty<bool> HasValidationErrorProperty =
        AvaloniaProperty.Register<BaseEditor, bool>(nameof(HasValidationError));

    /// <summary>Identifies the <see cref="ValidationErrorMessage"/> styled property.</summary>
    public static readonly StyledProperty<string?> ValidationErrorMessageProperty =
        AvaloniaProperty.Register<BaseEditor, string?>(nameof(ValidationErrorMessage));

    /// <summary>Identifies the <see cref="SelectAllTextOnFocus"/> styled property.</summary>
    public static readonly StyledProperty<bool> SelectAllTextOnFocusProperty =
        AvaloniaProperty.Register<BaseEditor, bool>(nameof(SelectAllTextOnFocus), defaultValue: true);

    /// <summary>Gets or sets the label displayed above or beside the editor.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets a unit label displayed adjacent to the input (e.g. "kg", "ms").</summary>
    public string? Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    /// <summary>Gets or sets content rendered in the leading (left) slot of the editor template.</summary>
    public object? LeadingContent
    {
        get => GetValue(LeadingContentProperty);
        set => SetValue(LeadingContentProperty, value);
    }

    /// <summary>Gets or sets content rendered in the trailing action slot of the editor template.</summary>
    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the editor currently has a validation error.
    /// Setting this to <see langword="true"/> activates the <c>:error</c> pseudo-class on the control.
    /// </summary>
    public bool HasValidationError
    {
        get => GetValue(HasValidationErrorProperty);
        set => SetValue(HasValidationErrorProperty, value);
    }

    /// <summary>Gets or sets the validation error message to display when <see cref="HasValidationError"/> is <see langword="true"/>.</summary>
    public string? ValidationErrorMessage
    {
        get => GetValue(ValidationErrorMessageProperty);
        set => SetValue(ValidationErrorMessageProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether all text in the editor is selected when the control receives focus.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool SelectAllTextOnFocus
    {
        get => GetValue(SelectAllTextOnFocusProperty);
        set => SetValue(SelectAllTextOnFocusProperty, value);
    }

    /// <summary>Stores the current parse error message, or <see langword="null"/> when the input is valid.</summary>
    private string? _parseError;

    /// <summary>Stores the current binding validation error message, or <see langword="null"/> when no binding error is present.</summary>
    private string? _bindingError;

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(BaseEditor);

    /// <summary>
    /// Initializes static members of <see cref="BaseEditor"/>.
    /// Registers a property-changed handler that keeps the <c>:error</c> pseudo-class in sync with <see cref="HasValidationError"/>.
    /// </summary>
    static BaseEditor()
    {
        HasValidationErrorProperty.Changed.AddClassHandler<BaseEditor>((editor, _) =>
        {
            editor.PseudoClasses.Set(":error", editor.HasValidationError);
        });
    }

    /// <summary>
    /// Called when the control receives keyboard focus. Selects all text if <see cref="SelectAllTextOnFocus"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="e">The focus event arguments.</param>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        if (SelectAllTextOnFocus)
            Dispatcher.UIThread.Post(SelectAll);
    }

    /// <summary>
    /// Overrides Avalonia's default data-validation adorner behaviour. Captures binding validation
    /// errors and routes them through <see cref="RefreshValidationState"/> instead.
    /// </summary>
    /// <param name="property">The property whose binding produced the validation result.</param>
    /// <param name="state">The type of binding value (e.g. data validation error).</param>
    /// <param name="error">The exception that represents the validation error, if any.</param>
    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        // Do NOT call base — suppresses Avalonia's default DataValidationErrors adorner.
        if (state is BindingValueType.DataValidationError or BindingValueType.DataValidationErrorWithFallback)
            _bindingError = ExtractErrorMessage(error);
        else
            _bindingError = null;

        RefreshValidationState();
    }

    /// <summary>
    /// Sets a parse error message and refreshes the validation state. Call this when user input cannot be parsed.
    /// </summary>
    /// <param name="message">The error message to display, or <see langword="null"/> to clear the parse error.</param>
    protected void SetParseError(string? message)
    {
        _parseError = message;
        RefreshValidationState();
    }

    /// <summary>
    /// Clears any current parse error and refreshes the validation state.
    /// </summary>
    protected void ClearParseError()
    {
        _parseError = null;
        RefreshValidationState();
    }

    /// <summary>
    /// Recomputes <see cref="HasValidationError"/> and <see cref="ValidationErrorMessage"/> from
    /// the current parse error and binding error, giving priority to the parse error.
    /// </summary>
    private void RefreshValidationState()
    {
        var activeError = _parseError ?? _bindingError;
        HasValidationError = activeError != null;
        ValidationErrorMessage = activeError;
    }

    /// <summary>
    /// Extracts a human-readable message from a validation exception, unwrapping
    /// single-inner <see cref="AggregateException"/> instances and preferring inner exception messages.
    /// </summary>
    /// <param name="error">The exception to extract a message from.</param>
    /// <returns>The extracted message string, or <see langword="null"/> if <paramref name="error"/> is <see langword="null"/>.</returns>
    private static string? ExtractErrorMessage(Exception? error)
    {
        if (error == null) return null;

        if (error is AggregateException agg && agg.InnerExceptions.Count == 1)
            error = agg.InnerExceptions[0];

        return error.InnerException?.Message ?? error.Message;
    }
}
