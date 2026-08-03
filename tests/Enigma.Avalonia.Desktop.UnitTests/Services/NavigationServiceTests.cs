using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Enigma.Avalonia.Desktop.Controls.Navigation;
using Enigma.Avalonia.Desktop.Services;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Services;

/// <summary>
/// Covers the navigation service: the page factory, the two lifecycle callbacks, the
/// cancel-on-false contract, the single-navigation lock, and the failure channel.
/// </summary>
public sealed class NavigationServiceTests
{
    [AvaloniaFact]
    public void SelectedItem_InvokesThePageFactoryForThatItem()
    {
        NavigationService service = new();
        NavigationItem item = new() { PageType = typeof(TestPageA) };
        TestPageA page = new();
        NavigationItem? factoryArgument = null;
        service.PageFactory = navItem =>
        {
            factoryArgument = navItem;
            return page;
        };
        service.Items.Add(item);

        service.SelectedItem = item;

        Assert.Same(item, factoryArgument);
        Assert.Same(page, service.CurrentPage);
    }

    /// <summary>
    /// The default factory builds both halves of the page — the view and its ViewModel — and wires
    /// them together, which is the only reason <c>PageViewModelType</c> exists.
    /// </summary>
    [AvaloniaFact]
    public void TheDefaultPageFactory_CreatesThePageAndSetsItsViewModelAsDataContext()
    {
        NavigationService service = new();
        NavigationItem item = new()
        {
            PageType = typeof(TestPageA),
            PageViewModelType = typeof(TestPageViewModel),
        };
        service.Items.Add(item);

        service.SelectedItem = item;

        TestPageA page = Assert.IsType<TestPageA>(service.CurrentPage);
        Assert.IsType<TestPageViewModel>(page.DataContext);
    }

    /// <summary>
    /// The service has no separate initialisation step; navigating to an item whose
    /// <c>PageType</c> was never configured is the equivalent failure, and it surfaces on the
    /// failure channel rather than as an unobserved exception.
    /// </summary>
    [AvaloniaFact]
    public void AnUnconfiguredNavigationItem_SurfacesThroughNavigationFailed()
    {
        NavigationService service = new();
        NavigationItem item = new();
        NavigationFailedEventArgs? failure = null;
        service.NavigationFailed += (_, e) => failure = e;
        service.Items.Add(item);

        service.SelectedItem = item;

        Assert.NotNull(failure);
        Assert.Equal("PageFactory", failure.Phase);
        Assert.IsType<ArgumentNullException>(failure.Exception);
        Assert.Null(service.CurrentPage);
    }

    [AvaloniaFact]
    public void APageFactoryThatThrows_ReportsTheFailureAndClearsTheCurrentPage()
    {
        NavigationService service = new();
        NavigationItem good = new() { PageType = typeof(TestPageA) };
        NavigationItem bad = new() { PageType = typeof(TestPageB) };
        TestPageA page = new();
        InvalidOperationException boom = new("no page for you");
        service.PageFactory = item => item == good ? page : throw boom;
        service.Items.Add(good);
        service.Items.Add(bad);
        NavigationFailedEventArgs? failure = null;
        service.NavigationFailed += (_, e) => failure = e;

        service.SelectedItem = good;
        Assert.Same(page, service.CurrentPage);

        service.SelectedItem = bad;

        Assert.NotNull(failure);
        Assert.Equal("PageFactory", failure.Phase);
        Assert.Same(boom, failure.Exception);
        Assert.Null(service.CurrentPage);
    }

    [AvaloniaFact]
    public async Task NavigateToAsync_PassesItsParameterToOnAppearingAsync()
    {
        NavigationService service = new();
        RecordingNavigationViewModel viewModel = new();
        TestPageA page = new() { DataContext = viewModel };

        await service.NavigateToAsync(page, "payload");

        Assert.Equal(1, viewModel.AppearingCalls);
        Assert.Equal("payload", viewModel.LastParameter);
    }

    /// <summary>
    /// Item-driven navigation has no parameter to pass — <c>NavigationItem</c> carries no payload,
    /// so the callback receives <see langword="null"/>. Only <c>NavigateToAsync</c> can supply one.
    /// </summary>
    [AvaloniaFact]
    public void ItemDrivenNavigation_CallsOnAppearingAsyncWithoutAParameter()
    {
        NavigationService service = new();
        RecordingNavigationViewModel viewModel = new();
        TestPageA page = new() { DataContext = viewModel };
        NavigationItem item = new() { PageType = typeof(TestPageA) };
        service.PageFactory = _ => page;
        service.Items.Add(item);

        service.SelectedItem = item;

        Assert.Equal(1, viewModel.AppearingCalls);
        Assert.Null(viewModel.LastParameter);
    }

    [AvaloniaFact]
    public async Task NavigateToAsync_SelectsTheItemMatchingThePageType()
    {
        NavigationService service = new();
        NavigationItem item = new() { PageType = typeof(TestPageA) };
        service.Items.Add(item);
        TestPageA page = new();

        await service.NavigateToAsync(page);

        Assert.Same(item, service.SelectedItem);
        Assert.Same(page, service.CurrentPage);
    }

    [AvaloniaFact]
    public async Task NavigateToAsync_AlsoMatchesFooterItems()
    {
        NavigationService service = new();
        NavigationItem settings = new() { PageType = typeof(TestPageB) };
        service.Items.Add(new NavigationItem { PageType = typeof(TestPageA) });
        service.FooterItems.Add(settings);

        await service.NavigateToAsync(new TestPageB());

        Assert.Same(settings, service.SelectedItem);
    }

    [AvaloniaFact]
    public async Task NavigateToAsync_LeavesTheSelectionNullWhenNoItemMatches()
    {
        NavigationService service = new();
        service.Items.Add(new NavigationItem { PageType = typeof(TestPageA) });

        await service.NavigateToAsync(new TestPageB());

        Assert.Null(service.SelectedItem);
    }

    /// <summary>A page that refuses to disappear keeps the current page in place.</summary>
    [AvaloniaFact]
    public async Task OnDisappearingAsyncReturningFalse_CancelsTheNavigation()
    {
        NavigationService service = new();
        RecordingNavigationViewModel blocking = new() { AllowDisappearing = false };
        TestPageA first = new() { DataContext = blocking };
        RecordingNavigationViewModel arriving = new();
        TestPageB second = new() { DataContext = arriving };
        await service.NavigateToAsync(first);

        await service.NavigateToAsync(second);

        Assert.Same(first, service.CurrentPage);
        Assert.Equal(1, blocking.DisappearingCalls);
        Assert.Equal(0, arriving.AppearingCalls);
    }

    /// <summary>
    /// A cancelled item navigation also rolls the selection back, so the navigation control does
    /// not end up highlighting a page that was never shown.
    /// </summary>
    [AvaloniaFact]
    public void OnDisappearingAsyncReturningFalse_RestoresThePreviousSelection()
    {
        NavigationService service = new();
        RecordingNavigationViewModel blocking = new() { AllowDisappearing = false };
        NavigationItem first = new() { PageType = typeof(TestPageA) };
        NavigationItem second = new() { PageType = typeof(TestPageB) };
        TestPageA firstPage = new() { DataContext = blocking };
        TestPageB secondPage = new();
        service.PageFactory = item => item == first ? firstPage : secondPage;
        service.Items.Add(first);
        service.Items.Add(second);
        service.SelectedItem = first;
        Assert.Same(firstPage, service.CurrentPage);

        service.SelectedItem = second;

        Assert.Same(first, service.SelectedItem);
        Assert.Same(firstPage, service.CurrentPage);
    }

    /// <summary>
    /// The <c>SemaphoreSlim</c> is taken with a zero timeout, so a navigation raised while another
    /// is still awaiting a lifecycle callback is dropped outright — not queued behind it.
    /// </summary>
    [AvaloniaFact]
    public async Task ANavigationRaisedWhileOneIsInFlight_IsDropped()
    {
        NavigationService service = new();
        TaskCompletionSource gate = new();
        RecordingNavigationViewModel slow = new() { AppearingGate = gate.Task };
        TestPageA first = new() { DataContext = slow };
        RecordingNavigationViewModel arriving = new();
        TestPageB second = new() { DataContext = arriving };

        Task inFlight = service.NavigateToAsync(first);
        Assert.False(inFlight.IsCompleted);
        Assert.Same(first, service.CurrentPage);

        await service.NavigateToAsync(second);

        Assert.Same(first, service.CurrentPage);
        Assert.Equal(0, arriving.AppearingCalls);

        gate.SetResult();
        await inFlight.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Same(first, service.CurrentPage);
    }

    /// <summary>Once the in-flight navigation completes the lock is released and the next one runs.</summary>
    [AvaloniaFact]
    public async Task AfterTheInFlightNavigationCompletes_TheNextOneIsAccepted()
    {
        NavigationService service = new();
        TaskCompletionSource gate = new();
        RecordingNavigationViewModel slow = new() { AppearingGate = gate.Task };
        TestPageA first = new() { DataContext = slow };
        TestPageB second = new();

        Task inFlight = service.NavigateToAsync(first);
        gate.SetResult();
        await inFlight.WaitAsync(TestContext.Current.CancellationToken);

        await service.NavigateToAsync(second);

        Assert.Same(second, service.CurrentPage);
    }

    [AvaloniaFact]
    public async Task OnAppearingAsyncThatThrows_SurfacesThroughNavigationFailed()
    {
        NavigationService service = new();
        InvalidOperationException boom = new("appearing failed");
        RecordingNavigationViewModel viewModel = new() { AppearingException = boom };
        TestPageA page = new() { DataContext = viewModel };
        NavigationFailedEventArgs? failure = null;
        service.NavigationFailed += (_, e) => failure = e;

        await service.NavigateToAsync(page);

        Assert.NotNull(failure);
        Assert.Equal("OnAppearingAsync", failure.Phase);
        Assert.Same(boom, failure.Exception);
        Assert.Same(page, service.CurrentPage);
    }

    /// <summary>
    /// A throwing <c>OnDisappearingAsync</c> is reported but treated as consent: a broken guard
    /// must not trap the user on the current page.
    /// </summary>
    [AvaloniaFact]
    public async Task OnDisappearingAsyncThatThrows_ReportsTheFailureAndAllowsTheNavigation()
    {
        NavigationService service = new();
        InvalidOperationException boom = new("disappearing failed");
        RecordingNavigationViewModel viewModel = new() { DisappearingException = boom };
        TestPageA first = new() { DataContext = viewModel };
        TestPageB second = new();
        await service.NavigateToAsync(first);
        NavigationFailedEventArgs? failure = null;
        service.NavigationFailed += (_, e) => failure = e;

        await service.NavigateToAsync(second);

        Assert.NotNull(failure);
        Assert.Equal("OnDisappearingAsync", failure.Phase);
        Assert.Same(boom, failure.Exception);
        Assert.Same(second, service.CurrentPage);
    }

    [AvaloniaFact]
    public void SelectingNothing_ClearsTheCurrentPage()
    {
        NavigationService service = new();
        NavigationItem item = new() { PageType = typeof(TestPageA) };
        service.PageFactory = _ => new TestPageA();
        service.Items.Add(item);
        service.SelectedItem = item;
        Assert.NotNull(service.CurrentPage);

        service.SelectedItem = null;

        Assert.Null(service.CurrentPage);
    }

    /// <summary>A page whose DataContext ignores the lifecycle interface navigates just the same.</summary>
    [AvaloniaFact]
    public async Task APageWithoutALifecycleViewModel_NavigatesWithoutCallbacks()
    {
        NavigationService service = new();
        TestPageA first = new();
        TestPageB second = new();

        await service.NavigateToAsync(first);
        await service.NavigateToAsync(second);

        Assert.Same(second, service.CurrentPage);
    }
}

/// <summary>A stand-in page view. Needs a public parameterless constructor for the default factory.</summary>
internal sealed class TestPageA : ContentControl;

/// <summary>A second stand-in page view, so page-type matching has something to distinguish.</summary>
internal sealed class TestPageB : ContentControl;

/// <summary>A stand-in page ViewModel for the default factory to instantiate.</summary>
internal sealed class TestPageViewModel;

/// <summary>
/// A configurable <see cref="INavigationViewModel"/> that records what the service called and can
/// be told to block, stall, or throw.
/// </summary>
internal sealed class RecordingNavigationViewModel : INavigationViewModel
{
    /// <summary>Gets the number of times <see cref="OnAppearingAsync"/> was called.</summary>
    public int AppearingCalls { get; private set; }

    /// <summary>Gets the number of times <see cref="OnDisappearingAsync"/> was called.</summary>
    public int DisappearingCalls { get; private set; }

    /// <summary>Gets the parameter the last <see cref="OnAppearingAsync"/> call received.</summary>
    public object? LastParameter { get; private set; }

    /// <summary>Gets or sets what <see cref="OnDisappearingAsync"/> answers. Defaults to allowing the navigation.</summary>
    public bool AllowDisappearing { get; set; } = true;

    /// <summary>Gets or sets a task that <see cref="OnAppearingAsync"/> waits on, to hold a navigation in flight.</summary>
    public Task? AppearingGate { get; set; }

    /// <summary>Gets or sets an exception <see cref="OnAppearingAsync"/> throws instead of completing.</summary>
    public Exception? AppearingException { get; set; }

    /// <summary>Gets or sets an exception <see cref="OnDisappearingAsync"/> throws instead of answering.</summary>
    public Exception? DisappearingException { get; set; }

    /// <inheritdoc />
    public async Task OnAppearingAsync(object? parameter = null)
    {
        AppearingCalls++;
        LastParameter = parameter;

        if (AppearingException is not null)
            throw AppearingException;

        if (AppearingGate is not null)
            await AppearingGate;
    }

    /// <inheritdoc />
    public Task<bool> OnDisappearingAsync()
    {
        DisappearingCalls++;

        if (DisappearingException is not null)
            throw DisappearingException;

        return Task.FromResult(AllowDisappearing);
    }
}
