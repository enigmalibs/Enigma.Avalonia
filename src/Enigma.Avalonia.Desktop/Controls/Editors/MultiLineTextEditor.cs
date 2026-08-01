using System;
using Avalonia.Controls;
using Avalonia.Media;

namespace Enigma.Avalonia.Desktop.Controls.Editors;

/// <summary>
/// A multi-line text editor that extends <see cref="BaseEditor"/> with return-key acceptance,
/// word-wrapping, and select-all-on-focus disabled by default.
/// </summary>
public class MultiLineTextEditor : BaseEditor
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(MultiLineTextEditor);

    /// <summary>
    /// Initializes a new instance of <see cref="MultiLineTextEditor"/> with
    /// <see cref="TextBox.AcceptsReturn"/> enabled, <see cref="BaseEditor.SelectAllTextOnFocus"/> disabled,
    /// and text wrapping set to <see cref="TextWrapping.Wrap"/>.
    /// </summary>
    public MultiLineTextEditor()
    {
        AcceptsReturn = true;
        SelectAllTextOnFocus = false;
        TextWrapping = TextWrapping.Wrap;
    }
}
