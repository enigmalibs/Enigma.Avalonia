using Enigma.Core.Encoding;

namespace Enigma.Avalonia.Desktop.Controls.Editors;

/// <summary>
/// A <see cref="ByteArrayEditor"/> that encodes and decodes byte arrays as Base64 strings
/// using <see cref="Base64Service"/>.
/// </summary>
public class Base64Editor : ByteArrayEditor
{
    /// <summary>Shared <see cref="Base64Service"/> instance used for encoding and decoding.</summary>
    private static readonly Base64Service Base64Service = new();

    /// <summary>
    /// Encodes <paramref name="value"/> as a Base64 string.
    /// </summary>
    /// <param name="value">The byte array to encode.</param>
    /// <returns>The Base64 string representation of <paramref name="value"/>.</returns>
    protected override string FormatValue(byte[] value)
        => Base64Service.Encode(value);

    /// <summary>
    /// Attempts to decode <paramref name="text"/> from Base64 into a byte array.
    /// </summary>
    /// <param name="text">The Base64 string to decode.</param>
    /// <param name="result">The decoded byte array when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if decoding succeeded; otherwise <see langword="false"/>.</returns>
    protected override bool TryParse(string? text, out byte[] result)
    {
        result = [];
        var success = false;

        if (text is null) return true;

        try
        {
            result = Base64Service.Decode(text);
            success = true;
        }
        catch
        {
            //ignored
        }

        return success;
    }
}
