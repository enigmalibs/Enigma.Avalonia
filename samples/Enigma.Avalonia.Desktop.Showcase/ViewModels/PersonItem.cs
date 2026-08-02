namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>One row of the sample collection the CollectionView page sorts, filters and groups.</summary>
/// <remarks>
/// A plain record with no change notification: <c>CollectionView</c> reads the properties it sorts and
/// groups by through reflection when it refreshes, so the items themselves need to implement nothing.
/// </remarks>
/// <param name="FirstName">The person's given name.</param>
/// <param name="LastName">The person's family name.</param>
/// <param name="Department">The department the person belongs to; the page groups on this.</param>
public sealed record PersonItem(string FirstName, string LastName, string Department);
