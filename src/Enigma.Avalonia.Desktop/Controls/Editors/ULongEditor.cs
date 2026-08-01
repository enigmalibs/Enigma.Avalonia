using System.Globalization;

namespace Enigma.Avalonia.Desktop.Controls.Editors;

/// <summary>
/// An editor for <see cref="ulong"/> (64-bit unsigned integer) values. Parses integer input using invariant culture
/// and formats the value with an optional <see cref="BaseEditor{T}.FormatString"/>.
/// </summary>
public class ULongEditor : BaseEditor<ulong>
{
    /// <summary>
    /// Attempts to parse <paramref name="text"/> as a <see cref="ulong"/> using invariant culture.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="result">The parsed value when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if parsing succeeded; otherwise <see langword="false"/>.</returns>
    protected override bool TryParse(string? text, out ulong result)
    {
        return ulong.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    /// <summary>
    /// Formats <paramref name="value"/> as a string using <see cref="BaseEditor{T}.FormatString"/> when set,
    /// or invariant culture default formatting otherwise.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted string representation of <paramref name="value"/>.</returns>
    protected override string FormatValue(ulong value)
    {
        if (!string.IsNullOrEmpty(FormatString))
            return value.ToString(FormatString, CultureInfo.InvariantCulture);

        return value.ToString(CultureInfo.InvariantCulture);
    }
}
