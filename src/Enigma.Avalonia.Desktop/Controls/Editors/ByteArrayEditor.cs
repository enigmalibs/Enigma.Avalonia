using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Enigma.Avalonia.Desktop.Controls.Editors;

/// <summary>
/// Abstract base class for editors that display and edit a <see cref="byte"/> array as encoded text
/// (e.g. hexadecimal or Base64). Extends <see cref="MultiLineTextEditor"/> with a <see cref="Value"/>
/// property and bidirectional synchronisation between the byte array and the text representation.
/// </summary>
public abstract class ByteArrayEditor : MultiLineTextEditor
{
    /// <summary>Identifies the <see cref="Value"/> styled property.</summary>
    public static readonly StyledProperty<byte[]?> ValueProperty =
        AvaloniaProperty.Register<ByteArrayEditor, byte[]?>(nameof(Value),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Guards against re-entrant synchronisation between <see cref="Value"/> and <see cref="TextBox.Text"/>.</summary>
    private bool _isSyncing;

    /// <summary>Gets or sets the byte array value represented by this editor.</summary>
    public byte[]? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(MultiLineTextEditor);

    /// <summary>
    /// Attempts to decode <paramref name="text"/> into a byte array using the encoding specific to this editor.
    /// </summary>
    /// <param name="text">The encoded text to decode.</param>
    /// <param name="result">The decoded byte array when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if decoding succeeded; otherwise <see langword="false"/>.</returns>
    protected abstract bool TryParse(string? text, out byte[] result);

    /// <summary>
    /// Encodes <paramref name="value"/> to its text representation using the encoding specific to this editor.
    /// </summary>
    /// <param name="value">The byte array to encode.</param>
    /// <returns>The encoded string representation of <paramref name="value"/>.</returns>
    protected abstract string FormatValue(byte[] value);

    /// <summary>
    /// Initializes static members of <see cref="ByteArrayEditor"/>.
    /// Registers property-changed handlers that keep <see cref="Value"/> and <see cref="TextBox.Text"/> in sync.
    /// </summary>
    static ByteArrayEditor()
    {
        ValueProperty.Changed.AddClassHandler<ByteArrayEditor>((editor, _) =>
        {
            editor.SyncTextFromValue();
        });

        TextProperty.Changed.AddClassHandler<ByteArrayEditor>((editor, _) =>
        {
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
            Text = Value is { } v ? FormatValue(v) : null;
            HasValidationError = false;
            ValidationErrorMessage = null;
        }
        finally
        {
            _isSyncing = false;
        }
    }

    /// <summary>
    /// Updates <see cref="Value"/> from the current <see cref="TextBox.Text"/>, setting a validation error
    /// if the text cannot be decoded.
    /// </summary>
    private void SyncValueFromText()
    {
        if (_isSyncing) return;
        _isSyncing = true;
        try
        {
            if (string.IsNullOrEmpty(Text))
            {
                Value = null;
                HasValidationError = false;
                ValidationErrorMessage = null;
            }
            else if (TryParse(Text, out var parsed))
            {
                Value = parsed;
                HasValidationError = false;
                ValidationErrorMessage = null;
            }
            else
            {
                HasValidationError = true;
                ValidationErrorMessage = $"Invalid value '{Text}'";
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    /// <summary>
    /// Called when the editor loses focus. Commits the current text, normalising the encoded representation if decoding succeeds.
    /// </summary>
    /// <param name="e">The routed event arguments.</param>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        CommitValue();
    }

    /// <summary>
    /// Finalises the current edit session. Clears the value for empty input, or parses and reformats
    /// the byte array from the current text on successful decode.
    /// </summary>
    private void CommitValue()
    {
        if (string.IsNullOrEmpty(Text))
        {
            Value = null;
            HasValidationError = false;
            ValidationErrorMessage = null;
            return;
        }

        if (TryParse(Text, out var parsed))
        {
            _isSyncing = true;
            try
            {
                Value = parsed;
                Text = FormatValue(parsed);
                HasValidationError = false;
                ValidationErrorMessage = null;
            }
            finally
            {
                _isSyncing = false;
            }
        }
        else
        {
            HasValidationError = true;
            ValidationErrorMessage = "Invalid value";
        }
    }
}
