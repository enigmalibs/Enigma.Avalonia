using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Enigma.Avalonia.Desktop.Data;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the data page: one collection, one view over it, sorted, filtered and grouped live.</summary>
/// <remarks>
/// <para>
/// The source stays an ordinary <see cref="ObservableCollection{T}"/> in its insertion order —
/// nothing here ever re-orders or removes from it. A single <see cref="CollectionViewSource"/> wraps
/// it, and every control on the page binds to <c>PeopleView.View</c>. Adding or removing a person
/// refreshes the view automatically, because the view subscribes to the source's
/// <c>CollectionChanged</c>.
/// </para>
/// <para>
/// Two of the three operations reconfigure themselves: <see cref="SortDescription"/> and
/// <see cref="PropertyGroupDescription"/> raise <c>DescriptionChanged</c>, and the source refreshes
/// the view in response — so changing the sort property or turning grouping on is a property
/// assignment, not a rebuild. Filtering is the exception and the one ordering rule worth knowing:
/// <c>CollectionViewSource</c> reads its <c>Filter</c> event once, when <c>Source</c> is assigned, so
/// the handler must be attached first. Afterwards, a filter change is a
/// <see cref="CollectionView.Refresh"/> call.
/// </para>
/// </remarks>
public class CollectionViewPageViewModel : ObservableObject
{
    /// <summary>The single sort criterion the page reconfigures in place.</summary>
    private readonly SortDescription _sort = new()
    {
        PropertyName = "LastName",
        Direction = SortDirection.Ascending,
    };

    /// <summary>The grouping criterion, added to and removed from the view as grouping is toggled.</summary>
    private readonly PropertyGroupDescription _group = new() { PropertyName = "Department" };

    /// <summary>Counts the people added from the page, so each gets a distinct name.</summary>
    private int _addedCount;

    /// <summary>Initializes a new instance of the <see cref="CollectionViewPageViewModel"/> class.</summary>
    public CollectionViewPageViewModel()
    {
        AddPersonCommand = new RelayCommand(OnAddPerson);
        RemoveLastCommand = new RelayCommand(OnRemoveLast, CanRemoveLast);
        ResetCommand = new RelayCommand(OnReset);

        PeopleView = new CollectionViewSource();

        // Before Source, deliberately: OnSourceChanged captures the Filter event once, and a handler
        // attached afterwards would never be consulted.
        PeopleView.Filter += OnFilter;
        PeopleView.Source = People;

        // After Source, equally deliberately: descriptions present at that moment get their change
        // handler attached twice, so the view would refresh twice per keystroke.
        PeopleView.SortDescriptions.Add(_sort);

        People.CollectionChanged += (_, _) =>
        {
            RemoveLastCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(TotalCount));
        };
    }

    /// <summary>Gets the source collection, in insertion order and untouched by the view.</summary>
    public ObservableCollection<PersonItem> People { get; } =
    [
        new("Alice", "Smith", "Engineering"),
        new("Bob", "Johnson", "Marketing"),
        new("Charlie", "Williams", "Engineering"),
        new("Diana", "Brown", "Sales"),
        new("Eve", "Jones", "Marketing"),
        new("Frank", "Garcia", "Engineering"),
        new("Grace", "Miller", "Sales"),
        new("Hank", "Davis", "Marketing"),
        new("Ivy", "Rodriguez", "Engineering"),
        new("Jack", "Wilson", "Sales"),
    ];

    /// <summary>Gets the view every list on the page binds through.</summary>
    public CollectionViewSource PeopleView { get; }

    /// <summary>Gets the property names offered in the sort picker.</summary>
    public IReadOnlyList<string> SortProperties { get; } = ["LastName", "FirstName", "Department"];

    /// <summary>Gets the number of items in the source, before filtering.</summary>
    public int TotalCount => People.Count;

    /// <summary>Gets or sets the text every person's name is matched against.</summary>
    public string FilterText
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                Refresh();
        }
    } = string.Empty;

    /// <summary>Gets or sets the property the view sorts on.</summary>
    public string SortProperty
    {
        get;
        set
        {
            // No Refresh() call: assigning PropertyName raises DescriptionChanged, and the source
            // refreshes the view for us.
            if (SetProperty(ref field, value))
                _sort.PropertyName = value;
        }
    } = "LastName";

    /// <summary>Gets or sets a value indicating whether the sort runs descending.</summary>
    public bool SortDescending
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                _sort.Direction = value ? SortDirection.Descending : SortDirection.Ascending;
        }
    }

    /// <summary>Gets or sets a value indicating whether the view is grouped by department.</summary>
    public bool IsGrouped
    {
        get;
        set
        {
            if (!SetProperty(ref field, value)) return;

            // Adding to or removing from GroupDescriptions refreshes the view; Groups goes from null
            // to a list of CollectionViewGroup, and back.
            if (value)
                PeopleView.GroupDescriptions.Add(_group);
            else
                PeopleView.GroupDescriptions.Remove(_group);
        }
    }

    /// <summary>Gets the command appending a new person to the source collection.</summary>
    public RelayCommand AddPersonCommand { get; }

    /// <summary>Gets the command removing the last person from the source collection.</summary>
    public RelayCommand RemoveLastCommand { get; }

    /// <summary>Gets the command clearing the filter and restoring the default sort and grouping.</summary>
    public RelayCommand ResetCommand { get; }

    /// <summary>Accepts or rejects one item against <see cref="FilterText"/>.</summary>
    /// <param name="sender">The collection view source raising the event.</param>
    /// <param name="e">The item under test, and the flag deciding its fate.</param>
    private void OnFilter(object? sender, FilterEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FilterText))
            return;

        e.Accepted = e.Item is PersonItem person
            && (person.FirstName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || person.LastName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || person.Department.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Appends a person, which the view picks up through the source's change notification.</summary>
    private void OnAddPerson()
    {
        _addedCount++;
        People.Add(new PersonItem(
            $"New{_addedCount}",
            $"Person{_addedCount}",
            _addedCount % 2 == 0 ? "Engineering" : "Sales"));
    }

    /// <summary>Removes the last person from the source, in source order rather than view order.</summary>
    private void OnRemoveLast()
    {
        if (People.Count > 0)
            People.RemoveAt(People.Count - 1);
    }

    /// <summary>Determines whether there is anything left to remove.</summary>
    /// <returns><see langword="true"/> when the source is not empty.</returns>
    private bool CanRemoveLast() => People.Count > 0;

    /// <summary>Restores the page's default filter, sort and grouping.</summary>
    private void OnReset()
    {
        FilterText = string.Empty;
        SortProperty = "LastName";
        SortDescending = false;
        IsGrouped = false;
    }

    /// <summary>Reapplies the filter to the view.</summary>
    private void Refresh() => PeopleView.View?.Refresh();
}
