using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the editors page: one bound value per editor type, plus a live readout of each.</summary>
/// <remarks>
/// <para>
/// An <see cref="ObservableValidator"/>, not an <c>ObservableObject</c>, and that is the point of the
/// page. Every value property carries DataAnnotations attributes and commits through
/// <c>SetProperty(ref field, value, true)</c>, so the toolkit validates on assignment and reports the
/// failures through <see cref="System.ComponentModel.INotifyDataErrorInfo"/>. Avalonia's binding layer
/// forwards those to the editor, which turns them into its <c>:error</c> pseudo-class and its message
/// line — the consumer writes attributes and bindings, never error-display code.
/// </para>
/// <para>
/// The paired <c>…Info</c> properties are set from the value setters and shown beside each editor.
/// They make the two-way binding visible, and they distinguish the two kinds of rejection: text the
/// editor cannot parse never reaches the ViewModel at all, so the readout still shows the last good
/// value, while an annotation violation does reach it — <c>SetProperty(…, true)</c> assigns and then
/// reports the error rather than vetoing the assignment.
/// </para>
/// </remarks>
public class EditorsTestingPageViewModel : ObservableValidator
{
    /// <summary>Gets the last accepted <see cref="TextValue"/>.</summary>
    public string? TextValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets the last accepted <see cref="IntValue"/>.</summary>
    public string? IntValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets the last accepted <see cref="UintValue"/>.</summary>
    public string? UintValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets the last accepted <see cref="ShortValue"/>.</summary>
    public string? ShortValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets the last accepted <see cref="UshortValue"/>.</summary>
    public string? UshortValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets the last accepted <see cref="LongValue"/>.</summary>
    public string? LongValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets the last accepted <see cref="UlongValue"/>.</summary>
    public string? UlongValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets the last accepted <see cref="SingleValue"/>.</summary>
    public string? SingleValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets the last accepted <see cref="DoubleValue"/>.</summary>
    public string? DoubleValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets the last accepted <see cref="DecimalValue"/>.</summary>
    public string? DecimalValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets the last accepted <see cref="MultiLineTextValue"/>.</summary>
    public string? MultiLineTextValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets the length of the last accepted <see cref="BytesValue"/>.</summary>
    public string? BytesValueInfo { get; private set => SetProperty(ref field, value); }

    /// <summary>Gets or sets the value edited by the single-line <c>TextEditor</c>s.</summary>
    [Required]
    [MaxLength(100)]
    public string? TextValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
                TextValueInfo = value ?? "null";
        }
    } = "Hello world";

    /// <summary>Gets or sets the value edited by the <c>IntEditor</c>.</summary>
    [Range(0, 10000)]
    public int? IntValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
                IntValueInfo = value?.ToString() ?? "null";
        }
    } = 42;

    /// <summary>Gets or sets the value edited by the <c>UIntEditor</c>.</summary>
    [Range(1u, 999999u)]
    public uint? UintValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
                UintValueInfo = value?.ToString() ?? "null";
        }
    } = 42;

    /// <summary>Gets or sets the value edited by the <c>ShortEditor</c>.</summary>
    /// <remarks>
    /// The one non-nullable value on the page: with <c>NullWhenEmpty</c> left at its default, clearing
    /// the editor commits <c>default(short)</c> rather than null.
    /// </remarks>
    [Range(-1000, 1000)]
    public short ShortValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
                ShortValueInfo = value.ToString();
        }
    } = -100;

    /// <summary>Gets or sets the value edited by the <c>UShortEditor</c>.</summary>
    [Range(1, 65535)]
    public ushort? UshortValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
                UshortValueInfo = value?.ToString() ?? "null";
        }
    } = 500;

    /// <summary>Gets or sets the value edited by the <c>LongEditor</c>.</summary>
    [Range(typeof(long), "0", "9223372036854775807")]
    public long? LongValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
                LongValueInfo = value?.ToString() ?? "null";
        }
    } = 9876543210;

    /// <summary>Gets or sets the value edited by the <c>ULongEditor</c>.</summary>
    [Range(typeof(ulong), "1", "1048576")]
    public ulong? UlongValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
                UlongValueInfo = value?.ToString() ?? "null";
        }
    } = 1024;

    /// <summary>Gets or sets the value edited by the <c>SingleEditor</c>.</summary>
    [Range(0.0, 100.0)]
    public float? SingleValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
                SingleValueInfo = value?.ToString() ?? "null";
        }
    } = 2.718f;

    /// <summary>Gets or sets the value edited by the <c>DoubleEditor</c>s.</summary>
    [Range(0.0, 99999.99)]
    public double? DoubleValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
                DoubleValueInfo = value?.ToString() ?? "null";
        }
    } = 3.14;

    /// <summary>Gets or sets the value edited by the <c>DecimalEditor</c>.</summary>
    [Range(typeof(decimal), "0.01", "99999.99")]
    public decimal? DecimalValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
                DecimalValueInfo = value?.ToString() ?? "null";
        }
    } = 99.95m;

    /// <summary>Gets or sets the value edited by the <c>MultiLineTextEditor</c>s.</summary>
    [Required]
    [MinLength(5)]
    public string? MultiLineTextValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
                MultiLineTextValueInfo = value ?? "null";
        }
    } = "Line one\nLine two\nLine three";

    /// <summary>Gets or sets the bytes edited by the <c>HexadecimalEditor</c> and the <c>Base64Editor</c>.</summary>
    /// <remarks>
    /// Bound to both binary editors at once, so typing hex into one re-renders the same bytes as
    /// Base64 in the other. No DataAnnotations here: what these two editors reject is unparseable
    /// text, which they report themselves as a parse error.
    /// </remarks>
    public byte[]? BytesValue
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                BytesValueInfo = value is not null ? $"{value.Length} bytes" : "null";
        }
    } = [0x0A, 0xFF, 0x1B, 0x42, 0xDE, 0xAD, 0xBE, 0xEF];
}
