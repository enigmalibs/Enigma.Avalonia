using System;
using System.Threading.Tasks;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests;

/// <summary>Assertions about tasks that are expected to complete on their own, such as a timed info bar.</summary>
internal static class TaskAssert
{
    /// <summary>
    /// How long a task gets before it is reported as never completing. Deliberately generous: it bounds a
    /// failing test, and a passing one returns as soon as its task completes.
    /// </summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    /// <summary>Asserts that a task completes within <see cref="Timeout"/>, and observes its outcome.</summary>
    /// <param name="task">The task expected to complete.</param>
    /// <returns>A task that completes once the assertion has been made.</returns>
    public static async Task CompletesAsync(Task task)
    {
        var winner = await Task.WhenAny(task, Task.Delay(Timeout, TestContext.Current.CancellationToken));

        Assert.Same(task, winner);
        await task;
    }
}
