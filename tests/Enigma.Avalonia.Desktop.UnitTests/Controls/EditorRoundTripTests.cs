using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Enigma.Avalonia.Desktop.Controls.Editors;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Controls;

/// <summary>
/// Each typed editor formats and parses its own type, and rejects text it cannot parse without
/// losing the value it already holds.
/// </summary>
/// <remarks>
/// Every editor is a <see cref="TextBox"/>, so these run headless; the value/text synchronisation
/// itself lives in static property-changed handlers and needs no template, which is why most cases
/// skip the window. The two that assert the <c>:error</c> state reaching the template do realise one.
/// </remarks>
public sealed class EditorRoundTripTests
{
    /// <summary>
    /// Asserts the three claims that define a typed editor: value formats to text, text parses back
    /// to the value, and unparseable text raises <c>:error</c> while leaving the value alone.
    /// </summary>
    /// <typeparam name="TEditor">The editor under test.</typeparam>
    /// <typeparam name="T">The value type it edits.</typeparam>
    /// <param name="value">A representative value.</param>
    /// <param name="text">The text that value formats to, invariant-culture.</param>
    /// <param name="invalidText">Text the editor cannot parse.</param>
    private static void AssertRoundTrip<TEditor, T>(T value, string text, string invalidText)
        where TEditor : BaseEditor<T>, new()
        where T : struct
    {
        TEditor formatting = new();
        formatting.Value = value;
        Assert.Equal(text, formatting.Text);
        Assert.False(formatting.HasValidationError);

        TEditor parsing = new();
        parsing.Text = text;
        Assert.Equal(value, parsing.Value);
        Assert.False(parsing.HasValidationError);

        TEditor rejecting = new();
        rejecting.Value = value;
        rejecting.Text = invalidText;
        Assert.True(rejecting.HasValidationError);
        Assert.Equal($"Invalid value '{invalidText}'", rejecting.ValidationErrorMessage);
        Assert.Equal(value, rejecting.Value);
        Assert.Contains(":error", rejecting.Classes);
    }

    [AvaloniaFact]
    public void IntEditor_RoundTripsItsValue() => AssertRoundTrip<IntEditor, int>(-42, "-42", "abc");

    [AvaloniaFact]
    public void UIntEditor_RoundTripsItsValue() => AssertRoundTrip<UIntEditor, uint>(4_000_000_000, "4000000000", "-1");

    [AvaloniaFact]
    public void LongEditor_RoundTripsItsValue() => AssertRoundTrip<LongEditor, long>(long.MinValue, "-9223372036854775808", "9223372036854775808");

    [AvaloniaFact]
    public void ULongEditor_RoundTripsItsValue() => AssertRoundTrip<ULongEditor, ulong>(ulong.MaxValue, "18446744073709551615", "-1");

    [AvaloniaFact]
    public void ShortEditor_RoundTripsItsValue() => AssertRoundTrip<ShortEditor, short>(-32_768, "-32768", "32768");

    [AvaloniaFact]
    public void UShortEditor_RoundTripsItsValue() => AssertRoundTrip<UShortEditor, ushort>(65_535, "65535", "65536");

    [AvaloniaFact]
    public void DoubleEditor_RoundTripsItsValue() => AssertRoundTrip<DoubleEditor, double>(-1.5, "-1.5", "1,5");

    [AvaloniaFact]
    public void SingleEditor_RoundTripsItsValue() => AssertRoundTrip<SingleEditor, float>(0.25f, "0.25", "nope");

    [AvaloniaFact]
    public void DecimalEditor_RoundTripsItsValue() => AssertRoundTrip<DecimalEditor, decimal>(19.99m, "19.99", "abc");

    /// <summary>
    /// <see cref="DecimalEditor"/> parses with <c>NumberStyles.Number</c>, which accepts invariant
    /// group separators — <c>"1,999.99"</c> is a valid input, not an error. Pinned because the sibling
    /// editors are stricter and a reader would reasonably expect them to agree.
    /// </summary>
    [AvaloniaFact]
    public void DecimalEditor_AcceptsInvariantGroupSeparators()
    {
        DecimalEditor editor = new();

        editor.Text = "1,999.99";

        Assert.False(editor.HasValidationError);
        Assert.Equal(1999.99m, editor.Value);
    }

    /// <summary>
    /// Parsing is invariant-culture, deliberately: a comma decimal separator is rejected rather than
    /// silently reinterpreted, so a value never changes magnitude because of the machine's locale.
    /// </summary>
    [AvaloniaFact]
    public void ADecimalCommaSeparator_IsRejectedRatherThanReinterpreted()
    {
        DoubleEditor editor = new();

        editor.Text = "1,5";

        Assert.True(editor.HasValidationError);
        Assert.Null(editor.Value);
    }

    /// <summary>The format string is applied when formatting, and the formatted text still parses back.</summary>
    [AvaloniaFact]
    public void TheFormatString_IsAppliedWhenFormatting()
    {
        DoubleEditor editor = new() { FormatString = "F2" };

        editor.Value = 1.5;

        Assert.Equal("1.50", editor.Text);
        Assert.Equal(1.5, editor.Value);
    }

    /// <summary>Clearing the text yields <c>default(T)</c> unless the editor opts into null.</summary>
    [AvaloniaFact]
    public void ClearingTheText_YieldsDefaultUnlessNullWhenEmptyIsSet()
    {
        IntEditor defaulting = new();
        defaulting.Value = 7;
        defaulting.Text = string.Empty;
        Assert.Equal(0, defaulting.Value);
        Assert.False(defaulting.HasValidationError);

        IntEditor nulling = new() { NullWhenEmpty = true };
        nulling.Value = 7;
        nulling.Text = string.Empty;
        Assert.Null(nulling.Value);
        Assert.False(nulling.HasValidationError);
    }

    /// <summary>Correcting invalid text clears the error and updates the value.</summary>
    [AvaloniaFact]
    public void CorrectingInvalidText_ClearsTheError()
    {
        IntEditor editor = new();
        editor.Text = "abc";
        Assert.True(editor.HasValidationError);

        editor.Text = "12";

        Assert.False(editor.HasValidationError);
        Assert.Null(editor.ValidationErrorMessage);
        Assert.Equal(12, editor.Value);
        Assert.DoesNotContain(":error", editor.Classes);
    }

    /// <summary>
    /// <see cref="Base64Editor"/> round-trips a byte array through <c>Enigma.Core</c>'s Base64 service.
    /// </summary>
    [AvaloniaFact]
    public void Base64Editor_RoundTripsAByteArray()
    {
        byte[] payload = [0x01, 0x02, 0x03, 0xFA];
        Base64Editor encoding = new();

        encoding.Value = payload;

        Assert.Equal("AQID+g==", encoding.Text);

        Base64Editor decoding = new();
        decoding.Text = "AQID+g==";

        Assert.Equal(payload, decoding.Value);
        Assert.False(decoding.HasValidationError);
    }

    /// <summary>
    /// <see cref="HexadecimalEditor"/> round-trips a byte array through <c>Enigma.Core</c>'s hex
    /// service. The encoder emits lower case; the decoder accepts either.
    /// </summary>
    [AvaloniaFact]
    public void HexadecimalEditor_RoundTripsAByteArray()
    {
        byte[] payload = [0x01, 0x02, 0x03, 0xFA];
        HexadecimalEditor encoding = new();

        encoding.Value = payload;

        Assert.Equal("010203fa", encoding.Text);

        HexadecimalEditor decoding = new();
        decoding.Text = "0A0B";

        Assert.Equal<byte[]>([0x0A, 0x0B], decoding.Value);
        Assert.False(decoding.HasValidationError);
    }

    /// <summary>Undecodable text raises the error and leaves the last good value in place.</summary>
    /// <param name="hex">Whether to exercise the hexadecimal editor rather than the Base64 one.</param>
    /// <param name="invalidText">Text the editor cannot decode.</param>
    [AvaloniaTheory]
    [InlineData(false, "!!!not base64!!!")]
    [InlineData(true, "ZZZZ")]
    public void AByteArrayEditor_RejectsUndecodableTextWithoutLosingItsValue(bool hex, string invalidText)
    {
        byte[] payload = [0x01, 0x02, 0x03];
        ByteArrayEditor editor = hex ? new HexadecimalEditor() : new Base64Editor();
        editor.Value = payload;

        editor.Text = invalidText;

        Assert.True(editor.HasValidationError);
        Assert.Equal($"Invalid value '{invalidText}'", editor.ValidationErrorMessage);
        Assert.Equal(payload, editor.Value);
        Assert.Contains(":error", editor.Classes);
    }

    /// <summary>Clearing a byte-array editor clears the value, with no error.</summary>
    [AvaloniaFact]
    public void ClearingAByteArrayEditor_ClearsItsValue()
    {
        Base64Editor editor = new();
        editor.Value = [1, 2, 3];

        editor.Text = string.Empty;

        Assert.Null(editor.Value);
        Assert.False(editor.HasValidationError);
    }

    /// <summary>
    /// The end-to-end claim: <c>:error</c> is not just a flag, it reaches the template — the error
    /// message becomes visible and the input border picks up the error brush.
    /// </summary>
    [AvaloniaFact]
    public void TheErrorState_ReachesTheTemplate()
    {
        IntEditor editor = new() { Title = "Amount" };
        var window = ControlCatalog.Realise(editor);
        var message = Part(editor, "PART_ErrorMessage");
        var border = (Border)Part(editor, "PART_Border");
        Assert.False(message.IsVisible);

        editor.Text = "abc";
        Dispatcher.UIThread.RunJobs();

        Assert.True(message.IsVisible);
        Application.Current!.TryFindResource("EnigmaErrorBrush", ThemeVariant.Dark, out var errorBrush);
        Assert.Same(errorBrush, border.BorderBrush);
        window.Close();
    }

    /// <summary>The message the template renders is the editor's own validation message.</summary>
    [AvaloniaFact]
    public void TheTemplateRendersTheValidationMessage()
    {
        IntEditor editor = new();
        var window = ControlCatalog.Realise(editor);

        editor.Text = "abc";
        Dispatcher.UIThread.RunJobs();

        var message = (TextBlock)Part(editor, "PART_ErrorMessage");
        Assert.Equal("Invalid value 'abc'", message.Text);
        window.Close();
    }

    /// <summary>Finds a named element of a control's own template.</summary>
    /// <param name="control">The templated control.</param>
    /// <param name="name">The part name.</param>
    /// <returns>The part.</returns>
    private static Control Part(Control control, string name) => control
        .GetVisualDescendants()
        .OfType<Control>()
        .Single(child => ReferenceEquals(child.TemplatedParent, control) && child.Name == name);
}
