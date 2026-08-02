using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Enigma.Avalonia.Desktop.Controls;
using Enigma.Avalonia.Desktop.Controls.ContentDialog;
using Enigma.Avalonia.Desktop.Controls.Docking;
using Enigma.Avalonia.Desktop.Controls.Editors;
using Enigma.Avalonia.Desktop.Controls.InfoBar;
using Enigma.Avalonia.Desktop.Controls.Navigation;
using Enigma.Avalonia.Desktop.Controls.Ribbon;

namespace Enigma.Avalonia.Desktop.UnitTests.Controls;

/// <summary>
/// One entry per templated control: how to build a representative instance, and the template parts
/// its <c>ControlTheme</c> is expected to expose.
/// </summary>
/// <param name="Name">The control's type name — the theory case identifier.</param>
/// <param name="Create">Builds an instance in the state a consumer would use it in.</param>
/// <param name="Parts">The <c>PART_*</c> names the control's own template must expose.</param>
internal sealed record ControlCase(string Name, Func<Control> Create, string[] Parts);

/// <summary>
/// The catalogue the headless smoke tests iterate. It is the test suite's answer to "which controls
/// does this library template?", and adding a control without adding it here is the gap the
/// <c>EveryShippedControlTheme_HasACatalogueEntry</c> test closes.
/// </summary>
internal static class ControlCatalog
{
    /// <summary>The parts both editor templates expose. They are structurally identical.</summary>
    /// <remarks>Declared before the catalogues below: static initialisers run in declaration order.</remarks>
    private static readonly string[] EditorParts =
    [
        "PART_Title", "PART_Border", "PART_LeadingContent", "PART_Watermark",
        "PART_TextPresenter", "PART_UnitLabel", "PART_ActionContent", "PART_ErrorMessage",
    ];

    /// <summary>Every control that ships a <c>ControlTheme</c> in <c>Fluent.axaml</c>.</summary>
    /// <remarks>
    /// The three overlay-style controls are created open: their themes set
    /// <c>IsVisible="False"</c> until <c>IsOpen</c> is true, and Avalonia never applies a template
    /// to a control it does not measure — so a closed dialog would "pass" a template test by never
    /// being templated at all.
    /// </remarks>
    public static readonly IReadOnlyList<ControlCase> All =
    [
        new("NavigationView",
            () => new NavigationView { Items = [new NavigationItem { Header = "Home" }], Logo = new TextBlock { Text = "Logo" } },
            ["PART_Border", "PART_Logo", "PART_ItemsListBox", "PART_FooterListBox"]),
        new("NavigationItem", () => new NavigationItem { Header = "Home" }, []),

        new("ContentDialog",
            () => new ContentDialog
            {
                IsOpen = true,
                Title = "Title",
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Maybe",
                CloseButtonText = "Cancel",
            },
            ["PART_Overlay", "PART_PrimaryButton", "PART_SecondaryButton", "PART_CloseButton"]),
        new("Overlay", () => new Overlay { IsOpen = true, Content = new TextBlock { Text = "Working…" } }, []),
        new("InfoBar", () => new InfoBar { IsOpen = true, Title = "Saved", Message = "All good" },
            ["PART_Card", "PART_Icon", "PART_CloseButton"]),

        new("SettingsCard", () => new SettingsCard { Header = "Theme", Description = "Pick one" },
            ["PART_Root", "PART_ContentPresenter", "PART_Chevron"]),
        new("SettingsCardExpander", () => new SettingsCardExpander { Header = "Advanced", IsExpanded = true },
            ["PART_Root", "PART_Header", "PART_Chevron", "PART_Separator", "PART_Content"]),

        new("Ribbon", () => { Ribbon ribbon = new(); ribbon.Tabs.Add(new RibbonTab { Header = "Home" }); return ribbon; },
            ["PART_TabStrip"]),
        new("RibbonTab", () => new RibbonTab { Header = "Home" }, []),
        new("RibbonGroup", () => new RibbonGroup { Header = "Clipboard" }, []),
        new("RibbonButton", () => new RibbonButton { Header = "Paste" }, ["PART_Root"]),
        new("RibbonToggleButton", () => new RibbonToggleButton { Header = "Bold" }, ["PART_Root"]),
        new("RibbonDropDownButton", () => new RibbonDropDownButton { Header = "Paste" }, ["PART_Root", "PART_Popup"]),

        new("DockingHost", () => new DockingHost(), ["PART_RootPanel", "PART_RootHost", "PART_DropOverlay"]),
        new("DockPane", () => new DockPane { Header = "Explorer" }, []),
        new("DockTabGroup",
            () => { DockTabGroup group = new(); group.Panes.Add(new DockPane { Header = "Explorer" }); return group; },
            ["PART_TabStrip"]),
        new("DockSplitContainer", () => new DockSplitContainer(),
            ["PART_Grid", "PART_First", "PART_Splitter", "PART_Second"]),

        new("BaseEditor", () => Editor(new BaseEditor()), EditorParts),
        new("MultiLineTextEditor", () => Editor(new MultiLineTextEditor()), EditorParts),
    ];

    /// <summary>
    /// The concrete editor subclasses. None ships a theme of its own — each resolves the
    /// <c>BaseEditor</c> or <c>MultiLineTextEditor</c> theme through its <c>StyleKeyOverride</c>,
    /// which is exactly the wiring worth a test.
    /// </summary>
    public static readonly IReadOnlyList<ControlCase> Editors =
    [
        new("TextEditor", () => Editor(new TextEditor()), EditorParts),
        new("IntEditor", () => Editor(new IntEditor()), EditorParts),
        new("UIntEditor", () => Editor(new UIntEditor()), EditorParts),
        new("LongEditor", () => Editor(new LongEditor()), EditorParts),
        new("ULongEditor", () => Editor(new ULongEditor()), EditorParts),
        new("ShortEditor", () => Editor(new ShortEditor()), EditorParts),
        new("UShortEditor", () => Editor(new UShortEditor()), EditorParts),
        new("DoubleEditor", () => Editor(new DoubleEditor()), EditorParts),
        new("SingleEditor", () => Editor(new SingleEditor()), EditorParts),
        new("DecimalEditor", () => Editor(new DecimalEditor()), EditorParts),
        new("Base64Editor", () => Editor(new Base64Editor()), EditorParts),
        new("HexadecimalEditor", () => Editor(new HexadecimalEditor()), EditorParts),
    ];

    /// <summary>Gets the catalogue entry with the given name.</summary>
    /// <param name="name">The control's type name.</param>
    /// <returns>The matching case.</returns>
    public static ControlCase Case(string name)
        => All.Concat(Editors).Single(entry => entry.Name == name);

    /// <summary>
    /// Puts a control in a shown headless window and runs the layout pass, so its template applies.
    /// </summary>
    /// <param name="control">The control to realise.</param>
    /// <returns>The window hosting it, so the caller can close it.</returns>
    public static Window Realise(Control control)
    {
        Window window = new() { Width = 900, Height = 700, Content = control };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    /// <summary>
    /// Gets the named elements of a control's <em>own</em> template.
    /// </summary>
    /// <remarks>
    /// Filtering on <c>TemplatedParent</c> is what makes this precise: a plain
    /// visual-descendant search would also match FluentTheme's own <c>PART_ContentPresenter</c> and
    /// <c>PART_TextPresenter</c> inside nested framework controls.
    /// </remarks>
    /// <param name="control">The templated control.</param>
    /// <returns>The part names, sorted.</returns>
    public static IReadOnlyList<string> TemplateParts(Control control) => control
        .GetVisualDescendants()
        .OfType<Control>()
        .Where(child => ReferenceEquals(child.TemplatedParent, control) && child.Name is not null)
        .Select(child => child.Name!)
        .OrderBy(name => name, StringComparer.Ordinal)
        .ToArray();

    /// <summary>Fills an editor's optional slots so every part of its template materialises.</summary>
    /// <typeparam name="T">The editor type.</typeparam>
    /// <param name="editor">The editor to configure.</param>
    /// <returns>The same editor.</returns>
    private static T Editor<T>(T editor) where T : BaseEditor
    {
        editor.Title = "Amount";
        editor.Unit = "kg";
        editor.LeadingContent = new TextBlock { Text = "€" };
        editor.ActionContent = new TextBlock { Text = "×" };
        editor.HasValidationError = true;
        editor.ValidationErrorMessage = "Invalid";
        return editor;
    }
}
