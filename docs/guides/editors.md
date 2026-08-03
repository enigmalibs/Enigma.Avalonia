# Editors

`Enigma.Avalonia.Desktop` provides thirteen ready-to-use input controls in the
`Enigma.Avalonia.Desktop.Controls.Editors` namespace. Every one derives from
`Avalonia.Controls.TextBox`, so caret handling, selection, clipboard, `MaxLength`, `PasswordChar`
and `IsReadOnly` behave exactly as on a stock text box. What the family adds is a title above the
input, a unit label and two content slots inside it, a validation message line below it, and — on
the typed editors — a strongly typed `Value` property the control keeps in sync with `Text`.

Pick the editor whose CLR type matches the property you are binding: `IntEditor` for an `int?`,
`DoubleEditor` for a `double?`, `HexadecimalEditor` or `Base64Editor` for a `byte[]?`, `TextEditor`
for a plain `string?`. Bind `Value` (or `Text`, for the two string editors) and set `Title`;
parsing, formatting, error state and error display are the control's job.

Parsing and formatting are invariant-culture throughout, and text that cannot be parsed is never
pushed to the binding. Type `abc` into an `IntEditor` holding `42` and the editor sets its `:error`
pseudo-class, renders `Invalid value 'abc'` under the box, and leaves `Value` at `42` — the
ViewModel never sees the bad input, and correcting the text clears the error on the next keystroke.

## Editors

| Editor | Bound member | CLR type | Parsing | Own properties |
|--------|--------------|----------|---------|----------------|
| `TextEditor` | `Text` | `string?` | none — raw text | none |
| `MultiLineTextEditor` | `Text` | `string?` | none — raw text | none; its constructor sets `AcceptsReturn = true`, `TextWrapping = Wrap`, `SelectAllTextOnFocus = false` |
| `ShortEditor` | `Value` | `short?` | `short.TryParse`, `NumberStyles.Integer` | none |
| `UShortEditor` | `Value` | `ushort?` | `ushort.TryParse`, `NumberStyles.Integer` | none |
| `IntEditor` | `Value` | `int?` | `int.TryParse`, `NumberStyles.Integer` | none |
| `UIntEditor` | `Value` | `uint?` | `uint.TryParse`, `NumberStyles.Integer` | none |
| `LongEditor` | `Value` | `long?` | `long.TryParse`, `NumberStyles.Integer` | none |
| `ULongEditor` | `Value` | `ulong?` | `ulong.TryParse`, `NumberStyles.Integer` | none |
| `SingleEditor` | `Value` | `float?` | `float.TryParse`, `NumberStyles.Float \| AllowLeadingSign` | none |
| `DoubleEditor` | `Value` | `double?` | `double.TryParse`, `NumberStyles.Float \| AllowLeadingSign` | none |
| `DecimalEditor` | `Value` | `decimal?` | `decimal.TryParse`, `NumberStyles.Number` | none |
| `ByteArrayEditor` (abstract) | `Value` | `byte[]?` | defined by the subclass | none |
| `HexadecimalEditor` | `Value` | `byte[]?` | `HexService.Decode` | none |
| `Base64Editor` | `Value` | `byte[]?` | `Base64Service.Decode` | none |

No editor declares a minimum, maximum or format property of its own. The only formatting knob is
`FormatString`, declared once on `BaseEditor<T>` and passed to the value's
`ToString(string, IFormatProvider)` overload with `CultureInfo.InvariantCulture`; the only emptiness
knob is `NullWhenEmpty`, also on `BaseEditor<T>`. Range checking is deliberately not a control
concern — it belongs to the bound property, and the editor renders whatever `INotifyDataErrorInfo`
reports (see [Validation from the ViewModel](#validation-from-the-viewmodel)). Input length is
capped with the inherited `TextBox.MaxLength`.

`DecimalEditor` is the one editor whose parse rule is looser than its siblings:
`NumberStyles.Number` accepts invariant group separators, so `1,999.99` is valid input. Every other
numeric editor rejects a comma outright rather than reinterpreting it, so a value can never change
magnitude because of the machine's locale.

## Key types

| Type | Namespace | Role |
|------|-----------|------|
| `BaseEditor` | `Enigma.Avalonia.Desktop.Controls.Editors` | Base of every editor. Extends `TextBox` with title, unit, leading/action slots, validation display and select-all-on-focus. Not abstract, but use `TextEditor` for plain text. |
| `BaseEditor<T>` | `Enigma.Avalonia.Desktop.Controls.Editors` | Abstract base for typed struct editors (`where T : struct`). Adds `Value`, `FormatString`, `NullWhenEmpty` and the `TryParse` / `FormatValue` extension points. |
| `ByteArrayEditor` | `Enigma.Avalonia.Desktop.Controls.Editors` | Abstract `MultiLineTextEditor` that edits a `byte[]?` as encoded text. |
| `TextEditor`, `MultiLineTextEditor` | `Enigma.Avalonia.Desktop.Controls.Editors` | The string editors; the multi-line one is also the base of the binary editors. |
| `Base64Service`, `HexService` | `Enigma.Core.Encoding` | Perform the encoding for `Base64Editor` and `HexadecimalEditor`. |

The Base64 and hexadecimal encoding is not hand-rolled here. `Base64Editor` and `HexadecimalEditor`
each hold a static `Enigma.Core.Encoding.Base64Service` / `HexService` from the **Enigma.Core**
package and call `Encode(byte[])` and `Decode(string)` on it. Enigma.Core is a package dependency of
`Enigma.Avalonia.Desktop`, so it arrives transitively — you never reference it yourself unless you
use it directly.

`BaseEditor` exposes these styled properties, all bindable:

| Property | Type | Default | Purpose |
|----------|------|---------|---------|
| `Title` | `string?` | `null` | Label rendered above the box. The row collapses entirely when null or empty. |
| `Unit` | `string?` | `null` | Trailing unit label (`kg`, `ms`, `px`) with a separator rule. Hidden when null or empty. |
| `PlaceholderText` | `string?` | `null` | Inherited from `TextBox`; rendered as the watermark while the editor is empty. |
| `LeadingContent` | `object?` | `null` | Arbitrary content in the leading slot, inside the border. Hidden when null. |
| `ActionContent` | `object?` | `null` | Arbitrary content in the trailing action slot, inside the border. Hidden when null. |
| `HasValidationError` | `bool` | `false` | Drives the `:error` pseudo-class. Settable, so a plain text editor can be error-flagged directly. |
| `ValidationErrorMessage` | `string?` | `null` | The message line's text. Visible only while `:error` is active. |
| `SelectAllTextOnFocus` | `bool` | `true` | Selects all text when the control gains focus. `MultiLineTextEditor` turns it off. |

`BaseEditor<T>` adds `Value` (`T?`, two-way by default, data-validation enabled), `FormatString`
(`string?`) and `NullWhenEmpty` (`bool`). `ByteArrayEditor` adds only `Value` (`byte[]?`, two-way).

## Usage

Each XAML file below is a normal Avalonia view with the usual `public partial class … : UserControl`
code-behind calling `InitializeComponent()`.

### Text editors

`TextEditor` and `MultiLineTextEditor` bind `Text`, not `Value` — there is nothing to parse.
`Title`, `PlaceholderText` and `Unit` are the three labels, and each disappears when left unset.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:editors="using:Enigma.Avalonia.Desktop.Controls.Editors"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.ProfileView"
             x:DataType="vm:ProfileViewModel">

  <StackPanel Margin="24" Spacing="16" MaxWidth="420">
    <editors:TextEditor Title="Display name"
                        PlaceholderText="Enter a name..."
                        Text="{Binding DisplayName}"
                        MaxLength="100" />

    <editors:MultiLineTextEditor Title="Notes"
                                 PlaceholderText="Anything worth remembering..."
                                 Text="{Binding Notes}"
                                 Unit="md"
                                 MinHeight="120" />
  </StackPanel>

</UserControl>
```

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyApp.ViewModels;

public class ProfileViewModel : ObservableObject
{
    public string? DisplayName { get; set => SetProperty(ref field, value); } = "Ada Lovelace";

    public string? Notes { get; set => SetProperty(ref field, value); }
}
```

### Typed numeric values

Bind `Value` and give the ViewModel property the matching nullable type. `FormatString` is applied
whenever the control renders the value — on assignment and again on focus loss — so an editor with
`FormatString="N2"` normalises `3.1` to `3.10` when you tab away. `NullWhenEmpty` decides what
clearing the box commits: `null` when on, `default(T)` when off.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:editors="using:Enigma.Avalonia.Desktop.Controls.Editors"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.MeasurementView"
             x:DataType="vm:MeasurementViewModel">

  <StackPanel Margin="24" Spacing="16" MaxWidth="420">
    <editors:IntEditor Title="Width"
                       PlaceholderText="Enter integer..."
                       Value="{Binding Width}"
                       Unit="px" />

    <editors:DoubleEditor Title="Distance"
                          Value="{Binding Distance}"
                          FormatString="N2"
                          Unit="mm" />

    <!-- Clearing this one commits null rather than 0, which is what the nullable type is for. -->
    <editors:UShortEditor Title="Port"
                          PlaceholderText="Leave empty for the default..."
                          Value="{Binding Port}"
                          NullWhenEmpty="True" />
  </StackPanel>

</UserControl>
```

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyApp.ViewModels;

public class MeasurementViewModel : ObservableObject
{
    public int? Width { get; set => SetProperty(ref field, value); } = 1920;

    public double? Distance { get; set => SetProperty(ref field, value); } = 3.14;

    public ushort? Port { get; set => SetProperty(ref field, value); } = 8080;
}
```

### Leading and action content

`LeadingContent` and `ActionContent` take any object and render it through a `ContentPresenter`
inside the editor's border — leading on the left, action on the right (below the text area on
`MultiLineTextEditor`). Both collapse when null. The action slot sets `Cursor="Arrow"`, so a button
placed there is clickable instead of fighting the I-beam over the text area.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:editors="using:Enigma.Avalonia.Desktop.Controls.Editors"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.SearchView"
             x:DataType="vm:SearchViewModel">

  <editors:TextEditor Title="Search"
                      PlaceholderText="Search..."
                      Text="{Binding Query}"
                      Margin="24"
                      MaxWidth="420">
    <editors:TextEditor.LeadingContent>
      <PathIcon Width="14"
                Height="14"
                Data="M 8,3 A 3,3 0 1 1 8,9 A 3,3 0 1 1 8,3 Z M 2,15 A 6,5 0 0 1 14,15 Z"
                Foreground="{DynamicResource EnigmaForegroundSecondaryBrush}" />
    </editors:TextEditor.LeadingContent>
    <editors:TextEditor.ActionContent>
      <Button Content="Go" Padding="6,2" FontSize="12" Command="{Binding SearchCommand}" />
    </editors:TextEditor.ActionContent>
  </editors:TextEditor>

</UserControl>
```

```csharp
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public class SearchViewModel : ObservableObject
{
    public SearchViewModel()
        => SearchCommand = new RelayCommand(() => Console.WriteLine($"Searching for '{Query}'"));

    public IRelayCommand SearchCommand { get; }

    public string? Query { get; set => SetProperty(ref field, value); }
}
```

### Binary values as hexadecimal and Base64

`HexadecimalEditor` and `Base64Editor` both edit a `byte[]?`. Bind the same property to both and
they mirror each other: type hex into one and the other re-renders the same bytes as Base64.
Neither control holds a string — the encoding is done by Enigma.Core's `HexService` and
`Base64Service`. The hex encoder emits lower case and its decoder accepts either case; the Base64
encoder emits canonical padded Base64 and its decoder tolerates embedded whitespace and line breaks.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:editors="using:Enigma.Avalonia.Desktop.Controls.Editors"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.PayloadView"
             x:DataType="vm:PayloadViewModel">

  <StackPanel Margin="24" Spacing="16" MaxWidth="520">
    <editors:HexadecimalEditor Title="Raw bytes"
                               PlaceholderText="Enter hex bytes (e.g. 0a1bff)..."
                               Value="{Binding Payload}" />

    <editors:Base64Editor Title="Encoded data"
                          PlaceholderText="Enter Base64 string..."
                          Value="{Binding Payload}" />
  </StackPanel>

</UserControl>
```

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyApp.ViewModels;

public class PayloadViewModel : ObservableObject
{
    public byte[]? Payload { get; set => SetProperty(ref field, value); }
        = [0x0A, 0xFF, 0x1B, 0x42, 0xDE, 0xAD, 0xBE, 0xEF];
}
```

### Validation from the ViewModel

`BaseEditor<T>.Value` and the inherited `TextBox.Text` are both registered with data validation
enabled, so anything the binding source reports through `INotifyDataErrorInfo` reaches the editor.
`BaseEditor` suppresses Avalonia's default `DataValidationErrors` adorner and renders the message in
its own line instead. With `CommunityToolkit.Mvvm` that means DataAnnotations on an
`ObservableValidator` plus `SetProperty(ref field, value, true)` — and no error-display code in the
view at all.

```csharp
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyApp.ViewModels;

public class RegistrationViewModel : ObservableValidator
{
    [Required]
    [MaxLength(100)]
    public string? UserName { get; set => SetProperty(ref field, value, true); } = "ada";

    [Range(1, 65535)]
    public int? Port { get; set => SetProperty(ref field, value, true); } = 8080;

    [Range(typeof(decimal), "0.01", "99999.99")]
    public decimal? Budget { get; set => SetProperty(ref field, value, true); } = 250.00m;
}
```

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:editors="using:Enigma.Avalonia.Desktop.Controls.Editors"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.RegistrationView"
             x:DataType="vm:RegistrationViewModel">

  <StackPanel Margin="24" Spacing="16" MaxWidth="420">
    <editors:TextEditor Title="User name" Text="{Binding UserName}" />
    <editors:IntEditor Title="Port" Value="{Binding Port}" />
    <editors:DecimalEditor Title="Budget" Value="{Binding Budget}" FormatString="N2" Unit="USD" />
  </StackPanel>

</UserControl>
```

The two rejection paths differ, and it is worth knowing which one you are looking at. Text the
editor cannot parse never reaches the ViewModel, so the bound property keeps its last good value
while the editor shows `Invalid value '<text>'`. An annotation violation does reach the ViewModel —
`SetProperty(…, true)` assigns and *then* reports — so the property holds the offending value and
the editor shows the annotation's message. When both are present the parse error wins.

For a `TextEditor` or `MultiLineTextEditor` validated against something the binding layer knows
nothing about, set `HasValidationError` and `ValidationErrorMessage` yourself; they are plain
settable styled properties and nothing on those two controls will overwrite them.

### Driving an editor from code

Every editor is an ordinary control: construct it, set properties, read the results.

```csharp
using System;
using Enigma.Avalonia.Desktop.Controls.Editors;

var editor = new DoubleEditor
{
    Title = "Distance",
    Unit = "mm",
    FormatString = "N2",
    NullWhenEmpty = true,
};

editor.Value = 1.5;
Console.WriteLine(editor.Text);                    // 1.50

editor.Text = "42.75";
Console.WriteLine(editor.Value);                   // 42.75

editor.Text = "1,5";                               // comma decimal separator — rejected
Console.WriteLine(editor.HasValidationError);      // True
Console.WriteLine(editor.ValidationErrorMessage);  // Invalid value '1,5'
Console.WriteLine(editor.Value);                   // 42.75 — the last good value survives

editor.Text = string.Empty;                        // NullWhenEmpty is on
Console.WriteLine(editor.Value.HasValue);          // False
Console.WriteLine(editor.HasValidationError);      // False
```

### Styling the error state

`HasValidationError` is mirrored onto the `:error` pseudo-class by a static property-changed
handler, so the whole error appearance is reachable from a style selector. The control theme uses it
to swap `PART_Border`'s brush to `EnigmaErrorBrush` and to reveal `PART_ErrorMessage`. Styles win
over control-theme setters, so a `Styles` file included in `Application.Styles` overrides any of it.

Note the `:is(...)` wrapper: a bare type selector matches that exact type only, so
`:is(editors|BaseEditor)` is what reaches all thirteen editors at once.

```xml
<Styles xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:editors="using:Enigma.Avalonia.Desktop.Controls.Editors">

  <!-- A thicker, tinted error border on every editor. -->
  <Style Selector=":is(editors|BaseEditor):error /template/ Border#PART_Border">
    <Setter Property="BorderThickness" Value="2" />
    <Setter Property="Background" Value="{DynamicResource EnigmaErrorBackgroundBrush}" />
  </Style>

  <!-- A louder message line. -->
  <Style Selector=":is(editors|BaseEditor):error /template/ TextBlock#PART_ErrorMessage">
    <Setter Property="FontWeight" Value="SemiBold" />
  </Style>

  <!-- Opt-in right alignment: put Classes="numeric" on any editor that should use it. -->
  <Style Selector=":is(editors|BaseEditor).numeric">
    <Setter Property="TextAlignment" Value="Right" />
  </Style>

</Styles>
```

The parts available to `/template/` selectors are `PART_Title`, `PART_Border`,
`PART_LeadingContent`, `PART_Watermark`, `PART_TextPresenter`, `PART_UnitLabel`,
`PART_ActionContent` and `PART_ErrorMessage`. `MultiLineTextEditor` — and therefore both binary
editors — has its own template with the same part names, laid out for a taller box. If you only
want a different error colour everywhere, redefine the `EnigmaErrorBrush` resource instead of
writing selectors; see [theming](theming.md).

### A custom typed editor

`BaseEditor<T>` is a public extension point: derive from it for any struct, implement `TryParse` and
`FormatValue`, and you inherit the chrome, the value/text synchronisation, the commit-on-focus-loss
reformat and the error plumbing. The control theme resolves through `BaseEditor`'s style key, so the
subclass is styled correctly with no extra XAML — use it exactly like a built-in editor, binding
`Value` to a `TimeSpan?`.

```csharp
using System;
using System.Globalization;
using Enigma.Avalonia.Desktop.Controls.Editors;

namespace MyApp.Controls;

public class TimeSpanEditor : BaseEditor<TimeSpan>
{
    protected override bool TryParse(string? text, out TimeSpan result)
        => TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out result);

    protected override string FormatValue(TimeSpan value)
        => string.IsNullOrEmpty(FormatString)
            ? value.ToString("c", CultureInfo.InvariantCulture)
            : value.ToString(FormatString, CultureInfo.InvariantCulture);
}
```

## Notes

- `SelectAllTextOnFocus` defaults to `true`, so clicking into an editor replaces its contents on the
  next keystroke. `MultiLineTextEditor` — and therefore `HexadecimalEditor` and `Base64Editor` —
  turns it off in its constructor. The selection is posted to the UI thread rather than applied
  inline, so it lands after Avalonia's own focus handling.
- Reformatting happens on focus loss, and only if the user actually edited the text. Tabbing through
  an untouched editor re-renders from the current `Value` and never re-parses, so a value cannot
  drift by being tabbed over.
- `ByteArrayEditor.Value` is registered *without* data validation, unlike `BaseEditor<T>.Value`.
  DataAnnotations on a `byte[]` property therefore do not reach `HexadecimalEditor` or
  `Base64Editor`; those two report only their own decode failures. `NullWhenEmpty` does not exist on
  them either — clearing a binary editor always commits `null`.
- The binary editors' message text differs by moment: while you type, undecodable text produces
  `Invalid value '<text>'`; committing on focus loss with the text still undecodable produces the
  shorter `Invalid value`.
- `BaseEditor.UpdateDataValidation` deliberately does not call its base implementation, suppressing
  Avalonia's default `DataValidationErrors` adorner so the editor's own message line is the single
  place errors appear. Nesting an editor inside a `DataValidationErrors` control will not produce
  the stock adorner.
- Messages arriving from a binding are unwrapped before display: a single-inner `AggregateException`
  is unwrapped and an inner exception's message is preferred over the outer one, which is what makes
  DataAnnotations messages arrive intact.
- `Title`, `Unit`, `LeadingContent` and `ActionContent` all collapse when unset, so a bare
  `<editors:TextEditor />` occupies exactly the height of the input box.
