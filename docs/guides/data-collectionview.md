# Collection views

`Enigma.Avalonia.Desktop` projects a source collection into a filtered, sorted and grouped *view*
without touching the collection itself. You wrap an `IEnumerable` in a `CollectionView`, describe
what you want with a filter predicate, `SortDescription` and `PropertyGroupDescription`, and bind
the result straight to any `ItemsControl`. The source keeps its own contents and its own order.

There are two entry points, one per idiom. `CollectionView` is the plain object you build and
refresh from code. `CollectionViewSource` is an `AvaloniaObject` wrapper around it, for XAML and for
ViewModels: it takes a bindable `Source`, hands back a live `View`, raises a `Filter` event once per
item, and re-runs the projection whenever a sort or group description changes.

The projection is rebuilt whole, never patched. Every refresh re-reads the source from the start,
re-applies the filter, re-sorts, re-groups, then reports itself as a single
`NotifyCollectionChangedAction.Reset` — there are no incremental notifications, and the view never
subscribes to the items themselves.

## Operations

| Operation | Configured with | Notes |
|-----------|-----------------|-------|
| Exclude items | `CollectionView.Filter`, or the `CollectionViewSource.Filter` event | Runs first, so sorting and grouping only ever see the survivors. `null` accepts everything. |
| Order items | `CollectionView.SortDescriptions` | A priority chain of `SortDescription`, each with its own `SortDirection`. |
| Group items | `CollectionView.GroupDescriptions`, read back through `Groups` | One `CollectionViewGroup` per distinct key. Only the first description is applied. |
| Rebuild the projection | `Refresh()` | The only thing that changes what the view contains. |
| Batch several changes | `DeferRefresh()` | Suspends refreshing until the returned token is disposed, then refreshes once. |
| Feed a control | Assign the view to `ItemsSource` | `CollectionView` is an `IList` *and* raises `INotifyCollectionChanged`, which is what Avalonia requires of an items source. |
| Inspect the projection | `Count`, `IsEmpty`, `this[int]`, `IndexOf`, `Contains`, `CopyTo`, `SourceCollection` | Read-only; every mutating `IList` member throws `NotSupportedException`. |

A refresh runs its three stages in a fixed order — filter, sort, group — and that order is the whole
contract. Property values are read by name through reflection over the item's public instance
properties, cached per type and name, so items need to implement nothing: a plain `record` works. A
name matching no property resolves to `null` for every item rather than throwing, which makes that
description inert instead of fatal. The sort runs through `List<T>.Sort`, so it is not stable, and
comparisons go through `Comparer.Default`, which places `null` ahead of every non-null value
ascending. Grouping applies `GroupDescriptions[0]` only — there is no nested-group model — and
orders groups by first appearance in the *already sorted* list, so the sort reorders the groups too.

## Key types

| Type | Namespace | Role |
|------|-----------|------|
| `CollectionView` | `Enigma.Avalonia.Desktop.Data` | The projection. `new CollectionView(source)`. Bindable as `ItemsSource`. |
| `CollectionViewSource` | `Enigma.Avalonia.Desktop.Data` | `AvaloniaObject` wrapper: `Source` in, `View` out, plus a `Filter` event. Declarable in XAML. |
| `SortDescription` | `Enigma.Avalonia.Desktop.Data` | One sort criterion: `PropertyName` + `Direction`. Raises `DescriptionChanged`. |
| `SortDirection` | `Enigma.Avalonia.Desktop.Data` | `Ascending` (the default) or `Descending`. |
| `PropertyGroupDescription` | `Enigma.Avalonia.Desktop.Data` | Group criterion: `PropertyName` + optional `ValueConverter`. Raises `DescriptionChanged`. |
| `CollectionViewGroup` | `Enigma.Avalonia.Desktop.Data` | One group: `Key`, `Items`, `ItemCount`. What `Groups` is a list of. |
| `FilterEventArgs` | `Enigma.Avalonia.Desktop.Data` | Carries `Item` into a `Filter` handler and `Accepted` (default `true`) back out. |

`SortDescriptions` and `GroupDescriptions` are `AvaloniaList<T>` from `Avalonia.Collections`. On
`CollectionView` both are settable, so a whole set of criteria can be swapped in one assignment; on
`CollectionViewSource` they are get-only and the wrapper hands those very instances to the view it
builds. There is no notion of currency — no `CurrentItem`, no `MoveCurrentTo*`; selection belongs to
the control. `Groups` is `null` until a group description is configured and an empty list once one
is, even if the filter rejected everything; grouping never touches the flat projection.

Knowing what does and does not trigger a rebuild is most of the subsystem:

| Change | Hand-built `CollectionView` | Through `CollectionViewSource` |
|--------|-----------------------------|--------------------------------|
| The source raises `CollectionChanged` | Refreshes | Refreshes |
| `Source` assigned | Not applicable | Builds a new view and refreshes it |
| A description added to or removed from `SortDescriptions` / `GroupDescriptions` | No refresh | Refreshes |
| `SortDescription.PropertyName` or `.Direction` changed | No refresh | Refreshes |
| `PropertyGroupDescription.PropertyName` or `.ValueConverter` changed | No refresh | Refreshes |
| `Filter` assigned, or the state a filter reads changed | No refresh | No refresh |
| A property of an item changed | No refresh | No refresh |

Everywhere the table says "no refresh", the fix is a `Refresh()` call. A source that does not
implement `INotifyCollectionChanged` — a plain `List<T>` — never drives the view on its own, and a
freshly constructed `CollectionView` is empty until its first `Refresh()`: the constructor
subscribes but does not project. `CollectionViewSource` hides that, refreshing the moment `Source`
is assigned. Passing `null` to the constructor throws `ArgumentNullException`.

## Usage

### Projecting, filtering and sorting a collection

Wrap the collection, refresh once, then enumerate it or bind it; because the source here is an
`ObservableCollection<T>`, later adds and removes reach the view on their own. `Filter` is a
`Predicate<object>` read fresh on every refresh, never captured, and it runs before the sort, so the
sort only ever orders survivors. Sort descriptions form a priority chain: the second breaks ties
left by the first, and reversing them changes the result. None of those assignments refreshes a
hand-built view by itself, which makes them a natural fit for `DeferRefresh` — the token suspends
refreshing and rebuilds once on dispose. Tokens nest, only the outermost dispose refreshes, and
disposing one twice is a no-op.

```csharp
using System;
using System.Collections.ObjectModel;
using Enigma.Avalonia.Desktop.Data;

ObservableCollection<Person> people =
[
    new("Alice", "Smith", "Engineering"),
    new("Bob", "Johnson", "Marketing"),
];

CollectionView view = new(people);
view.Refresh();                    // the constructor subscribes but does not project

foreach (Person person in view)
    Console.WriteLine($"{person.LastName}, {person.FirstName}");

people.Add(new Person("Carol", "Williams", "Engineering"));
Console.WriteLine(view.Count);     // 3 — the source notified the view, which refreshed itself

using (view.DeferRefresh())
{
    view.Filter = item => ((Person)item).LastName.Length > 5;
    view.SortDescriptions.Add(new SortDescription { PropertyName = nameof(Person.Department) });
    view.SortDescriptions.Add(new SortDescription
    {
        PropertyName = nameof(Person.LastName),
        Direction = SortDirection.Descending,
    });

    Console.WriteLine(view.Count); // 3 — still the pre-defer projection
}

Console.WriteLine(view.Count);     // 2 — one Reset was raised, on dispose

foreach (Person person in view)
    Console.WriteLine($"{person.Department} — {person.LastName}");

public sealed record Person(string FirstName, string LastName, string Department);
```

`Person` is the model behind every snippet in this guide; in an app it lives beside the ViewModels,
in the namespace the XAML below reaches through `xmlns:vm`. It implements no interface and raises no
notification, which is the point: the view reads its properties by reflection on each refresh.

### Grouping

`Groups` is filled by the same refresh that produces the flat view; each `CollectionViewGroup`
carries its `Key`, its `Items` and an `ItemCount`. A `ValueConverter` on the description turns the
raw property value into the key, which is how you group by something coarser than the property.

```csharp
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;
using Enigma.Avalonia.Desktop.Data;

ObservableCollection<Person> people =
[
    new("Alice", "Smith", "Engineering"),
    new("Bob", "Johnson", "Marketing"),
    new("Carol", "Williams", "Engineering"),
];

CollectionView view = new(people);
view.GroupDescriptions.Add(new PropertyGroupDescription
{
    PropertyName = nameof(Person.Department),
    ValueConverter = new InitialConverter(), // optional: group by "E" and "M", not the full name
});
view.Refresh();

foreach (CollectionViewGroup group in view.Groups ?? [])
{
    Console.WriteLine($"{group.Key} ({group.ItemCount})");

    foreach (Person person in group.Items)
        Console.WriteLine($"    {person.LastName}");
}

public sealed class InitialConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string text && text.Length > 0 ? text[..1] : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

The converter is called with `typeof(object)` as the target type, a `null` parameter and
`CultureInfo.CurrentCulture`. A key that comes out `null` becomes `string.Empty`, since the key
backs a dictionary lookup; a description with no `PropertyName` groups by the item itself.

### Declaring a `CollectionViewSource` in XAML

A `CollectionViewSource` is an ordinary resource. Its `Source` binds against the enclosing view's
`DataContext`, its descriptions are child elements, and its `Filter` event takes a code-behind
handler. Bind controls to `View` through `Source={StaticResource …}`, or to `View.Groups` for the
grouped shape, where every item is a `CollectionViewGroup`.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:data="using:Enigma.Avalonia.Desktop.Data"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.PeopleView"
             x:DataType="vm:PeopleViewModel">

  <UserControl.Resources>
    <!-- Filter is declared before Source: the wrapper reads the event once, as Source is assigned. -->
    <data:CollectionViewSource x:Key="PeopleSource"
                               Filter="OnFilter"
                               Source="{Binding People}">
      <data:CollectionViewSource.SortDescriptions>
        <data:SortDescription PropertyName="LastName" Direction="Ascending" />
      </data:CollectionViewSource.SortDescriptions>
      <data:CollectionViewSource.GroupDescriptions>
        <data:PropertyGroupDescription PropertyName="Department" />
      </data:CollectionViewSource.GroupDescriptions>
    </data:CollectionViewSource>
  </UserControl.Resources>

  <StackPanel Spacing="12" Margin="16">
    <TextBlock Text="{Binding View.Count, Source={StaticResource PeopleSource}}" />
    <ListBox ItemsSource="{Binding View, Source={StaticResource PeopleSource}}">
      <ListBox.ItemTemplate>
        <DataTemplate x:DataType="vm:Person">
          <TextBlock Text="{Binding LastName}" />
        </DataTemplate>
      </ListBox.ItemTemplate>
    </ListBox>

    <ItemsControl ItemsSource="{Binding View.Groups, Source={StaticResource PeopleSource}}">
      <ItemsControl.ItemTemplate>
        <DataTemplate x:DataType="data:CollectionViewGroup">
          <StackPanel Spacing="4" Margin="0,0,0,8">
            <TextBlock FontWeight="SemiBold"><Run Text="{Binding Key}" /><Run Text=" (" /><Run Text="{Binding ItemCount}" /><Run Text=")" /></TextBlock>
            <ItemsControl ItemsSource="{Binding Items}">
              <ItemsControl.ItemTemplate>
                <DataTemplate x:DataType="vm:Person">
                  <TextBlock Margin="12,0,0,0" Text="{Binding LastName}" />
                </DataTemplate>
              </ItemsControl.ItemTemplate>
            </ItemsControl>
          </StackPanel>
        </DataTemplate>
      </ItemsControl.ItemTemplate>
    </ItemsControl>
  </StackPanel>

</UserControl>
```

The handler receives one `FilterEventArgs` per item per refresh. `Accepted` starts at `true`, so
returning without touching it keeps the item.

```csharp
using Avalonia.Controls;
using Enigma.Avalonia.Desktop.Data;
using MyApp.ViewModels;

namespace MyApp.Views;

public partial class PeopleView : UserControl
{
    public PeopleView() => InitializeComponent();

    private void OnFilter(object? sender, FilterEventArgs e)
        => e.Accepted = e.Item is Person person && person.Department == "Engineering";
}
```

### Driving the view from a ViewModel

The same wrapper works entirely from code, which is what you want when the filter reads ViewModel
state. The two ordering rules in the constructor are the only sharp edges in the type. A view binds
to it with `ItemsSource="{Binding PeopleView.View}"`, `Text="{Binding PeopleView.View.Count}"`, and
`{Binding PeopleView.View.Groups}` for the grouped shape.

```csharp
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Enigma.Avalonia.Desktop.Data;

namespace MyApp.ViewModels;

public class PeopleViewModel : ObservableObject
{
    private readonly SortDescription _sort = new()
    {
        PropertyName = nameof(Person.LastName),
        Direction = SortDirection.Ascending,
    };

    public PeopleViewModel()
    {
        AddPersonCommand = new RelayCommand(OnAddPerson);

        PeopleView = new CollectionViewSource();

        // Before Source: the Filter event is read once, while Source is being assigned.
        PeopleView.Filter += OnFilter;
        PeopleView.Source = People;

        // After Source: a description already in the list when Source is assigned gets its change
        // handler attached twice, and every later edit to it then refreshes the view twice.
        PeopleView.SortDescriptions.Add(_sort);
    }

    public ObservableCollection<Person> People { get; } = [];

    public CollectionViewSource PeopleView { get; }

    public IRelayCommand AddPersonCommand { get; }

    public string FilterText
    {
        get;
        set
        {
            // Filter state is the one thing the wrapper cannot observe. Refresh explicitly.
            if (SetProperty(ref field, value))
                PeopleView.View?.Refresh();
        }
    } = string.Empty;

    public bool SortDescending
    {
        get;
        set
        {
            // No Refresh here: SortDescription raises DescriptionChanged, the wrapper reacts.
            if (SetProperty(ref field, value))
                _sort.Direction = value ? SortDirection.Descending : SortDirection.Ascending;
        }
    }

    private void OnFilter(object? sender, FilterEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FilterText))
            return;

        e.Accepted = e.Item is Person person
            && person.LastName.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
    }

    private void OnAddPerson() => People.Add(new Person("New", "Person", "Sales"));
}
```

## Notes

- A refresh raises exactly one `CollectionChanged` with `NotifyCollectionChangedAction.Reset`,
  followed by `PropertyChanged` for `Count`, `IsEmpty` and `Groups`, in that order. A bound control
  rebuilds its containers each time, so refreshing per keystroke over a large source is the cost to
  watch — that is what `DeferRefresh` is for.
- The view is a read-only `IList`: `Add`, `Insert`, `Remove`, `RemoveAt`, `Clear` and the indexer
  setter all throw `NotSupportedException`. Change the source collection, or the filter, instead.
  `IsReadOnly` is `true`, `IsFixedSize` is `false`, and `IsSynchronized` is `false` — the view is not
  thread-safe and expects the UI thread.
- Assigning `CollectionViewSource.Source` a second time builds a brand new `CollectionView` and
  detaches the old one from its source. Anything holding the previous `View` is stale, so re-read
  `View` rather than caching it. Assigning `null` leaves `View` as `null`.
- A `Filter` handler attached to a `CollectionViewSource` *after* `Source` was assigned is never
  consulted, and nothing is raised to tell you. Subscribe first.
- Sorting is not stable: add a tie-breaking `SortDescription` when the order of equal keys matters.
