using System.Globalization;

namespace Enigma.Avalonia.Desktop.Controls.Editors;

/// <summary>
/// An editor for <see cref="short"/> (16-bit signed integer) values. Parses integer input using invariant culture
/// and formats the value with an optional <see cref="BaseEditor{T}.FormatString"/>.
/// </summary>
public class ShortEditor : BaseEditor<short>
{
    /// <summary>
    /// Attempts to parse <paramref name="text"/> as a <see cref="short"/> using invariant culture.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="result">The parsed value when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if parsing succeeded; otherwise <see langword="false"/>.</returns>
    protected override bool TryParse(string? text, out short result)
    {
        return short.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    /// <summary>
    /// Formats <paramref name="value"/> as a string using <see cref="BaseEditor{T}.FormatString"/> when set,
    /// or invariant culture default formatting otherwise.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted string representation of <paramref name="value"/>.</returns>
    protected override string FormatValue(short value)
    {
        if (!string.IsNullOrEmpty(FormatString))
            return value.ToString(FormatString, CultureInfo.InvariantCulture);

        return value.ToString(CultureInfo.InvariantCulture);
    }
}
