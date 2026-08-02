using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Enigma.Avalonia.Desktop.Controls.Editors;

#pragma warning disable AVP1002 // Properties are accessed via non-generic concrete types in XAML
/// <summary>
/// Abstract base class for typed struct editors. Keeps a strongly-typed <see cref="Value"/> property
/// in sync with the underlying <see cref="TextBox.Text"/>, performing parsing on text change and
/// reformatting on focus loss.
/// </summary>
/// <typeparam name="T">The value type this editor edits. Must be a struct.</typeparam>
public abstract class BaseEditor<T> : BaseEditor where T : struct
{
    /// <summary>Identifies the <see cref="Value"/> styled property.</summary>
    public static readonly StyledProperty<T?> ValueProperty =
        AvaloniaProperty.Register<BaseEditor<T>, T?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    /// <summary>Identifies the <see cref="FormatString"/> styled property.</summary>
    public static readonly StyledProperty<string?> FormatStringProperty =
        AvaloniaProperty.Register<BaseEditor<T>, string?>(nameof(FormatString));

    /// <summary>Identifies the <see cref="NullWhenEmpty"/> styled property.</summary>
    public static readonly StyledProperty<bool> NullWhenEmptyProperty =
        AvaloniaProperty.Register<BaseEditor<T>, bool>(nameof(NullWhenEmpty));
#pragma warning restore AVP1002

    /// <summary>Guards against re-entrant synchronisation between <see cref="Value"/> and <see cref="TextBox.Text"/>.</summary>
    private bool _isSyncing;

    /// <summary>Tracks whether the user has modified the text since the last focus or value change.</summary>
    private bool _textModifiedByUser;

    /// <summary>Gets or sets the typed value represented by this editor.</summary>
    public T? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Gets or sets an optional format string passed to <see cref="FormatValue"/> when rendering
    /// the value as text (e.g. <c>"F2"</c> for two decimal places).
    /// </summary>
    public string? FormatString
    {
        get => GetValue(FormatStringProperty);
        set => SetValue(FormatStringProperty, value);
    }

    /// <summary>
    /// When true, clearing the text sets Value to null.
    /// When false (default), clearing the text sets Value to default(T).
    /// </summary>
    public bool NullWhenEmpty
    {
        get => GetValue(NullWhenEmptyProperty);
        set => SetValue(NullWhenEmptyProperty, value);
    }

    /// <summary>
    /// Attempts to parse <paramref name="text"/> into a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="text">The raw text to parse.</param>
    /// <param name="result">The parsed value when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if parsing succeeded; otherwise <see langword="false"/>.</returns>
    protected abstract bool TryParse(string? text, out T result);

    /// <summary>
    /// Converts <paramref name="value"/> to its display string, applying <see cref="FormatString"/> when set.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted string representation of <paramref name="value"/>.</returns>
    protected abstract string FormatValue(T value);

    /// <summary>
    /// Initializes static members of <see cref="BaseEditor{T}"/>.
    /// Registers property-changed handlers that keep <see cref="Value"/> and <see cref="TextBox.Text"/> in sync.
    /// </summary>
    static BaseEditor()
    {
        ValueProperty.Changed.AddClassHandler<BaseEditor<T>>((editor, _) =>
        {
            editor.SyncTextFromValue();
        });

        TextProperty.Changed.AddClassHandler<BaseEditor<T>>((editor, _) =>
        {
            if (!editor._isSyncing)
                editor._textModifiedByUser = true;
            editor.SyncValueFromText();
        });
    }

    /// <summary>
    /// Updates <see cref="TextBox.Text"/> to reflect the current <see cref="Value"/>, suppressing the
    /// reciprocal text-to-value synchronisation.
    /// </summary>
    private void SyncTextFromValue()
    {
        if (_isSyncing) return;
        _isSyncing = true;
        try
        {
            Text = Value.HasValue ? FormatValue(Value.Value) : null;
            ClearParseError();
        }
        finally
        {
            _isSyncing = false;
        }
    }

    /// <summary>
    /// Updates <see cref="Value"/> from the current <see cref="TextBox.Text"/>, setting a parse error
    /// if the text cannot be converted.
    /// </summary>
    private void SyncValueFromText()
    {
        if (_isSyncing) return;
        _isSyncing = true;
        try
        {
            if (string.IsNullOrEmpty(Text))
            {
                Value = NullWhenEmpty ? null : default(T);
                ClearParseError();
            }
            else if (TryParse(Text, out var parsed))
            {
                Value = parsed;
                ClearParseError();
            }
            else
            {
                SetParseError($"Invalid value '{Text}'");
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    /// <summary>
    /// Called when the editor loses focus. Commits the current text, reformatting the value if parsing succeeds.
    /// </summary>
    /// <param name="e">The routed event arguments.</param>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        CommitValue();
    }

    /// <summary>
    /// Finalises the current edit session. If the user modified the text, parses and reformats the value;
    /// otherwise, simply reformats from the existing <see cref="Value"/>.
    /// </summary>
    private void CommitValue()
    {
        if (!_textModifiedByUser)
        {
            // User didn't edit — just reformat from current Value
            _isSyncing = true;
            try
            {
                Text = Value.HasValue ? FormatValue(Value.Value) : null;
            }
            finally
            {
                _isSyncing = false;
            }
            return;
        }

        _textModifiedByUser = false;

        if (string.IsNullOrEmpty(Text))
        {
            Value = NullWhenEmpty ? null : default(T);
            ClearParseError();
            return;
        }

        if (TryParse(Text, out var parsed))
        {
            _isSyncing = true;
            try
            {
                Value = parsed;
                Text = FormatValue(parsed);
                ClearParseError();
            }
            finally
            {
                _isSyncing = false;
            }
        }
        else
        {
            SetParseError($"Invalid value '{Text}'");
        }
    }
}
