using Enigma.Core.Encoding;

namespace Enigma.Avalonia.Desktop.Controls.Editors;

/// <summary>
/// A <see cref="ByteArrayEditor"/> that encodes and decodes byte arrays as hexadecimal strings
/// using <see cref="HexService"/>.
/// </summary>
public class HexadecimalEditor : ByteArrayEditor
{
    /// <summary>Shared <see cref="HexService"/> instance used for encoding and decoding.</summary>
    private static readonly HexService HexService = new();

    /// <summary>
    /// Encodes <paramref name="value"/> as a hexadecimal string.
    /// </summary>
    /// <param name="value">The byte array to encode.</param>
    /// <returns>The hexadecimal string representation of <paramref name="value"/>.</returns>
    protected override string FormatValue(byte[] value)
        => HexService.Encode(value);

    /// <summary>
    /// Attempts to decode <paramref name="text"/> from hexadecimal into a byte array.
    /// </summary>
    /// <param name="text">The hexadecimal string to decode.</param>
    /// <param name="result">The decoded byte array when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if decoding succeeded; otherwise <see langword="false"/>.</returns>
    protected override bool TryParse(string? text, out byte[] result)
    {
        result = [];
        var success = false;

        if (text is null) return true;

        try
        {
            result = HexService.Decode(text);
            success = true;
        }
        catch
        {
            //ignored
        }

        return success;
    }
}
