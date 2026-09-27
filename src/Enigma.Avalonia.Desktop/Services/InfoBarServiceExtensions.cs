using System;
using System.Threading.Tasks;
using Enigma.Avalonia.Desktop.Controls.InfoBar;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Provides convenient extension methods for <see cref="IInfoBarService"/>, such as showing an info bar
/// that closes itself after a given period.
/// </summary>
public static class InfoBarServiceExtensions
{
    extension(IInfoBarService service)
    {
        /// <summary>
        /// Shows an info bar that closes itself once <paramref name="displayDuration"/> has elapsed, unless
        /// it is dismissed first.
        /// </summary>
        /// <remarks>
        /// <paramref name="configure"/> runs first and <paramref name="displayDuration"/> is applied after it,
        /// so the duration passed here wins over any <see cref="InfoBar.DisplayDuration"/> the action sets.
        /// </remarks>
        /// <param name="displayDuration">
        /// How long the info bar stays open: greater than <see cref="TimeSpan.Zero"/> and at most
        /// <see cref="int.MaxValue"/> milliseconds.
        /// </param>
        /// <param name="configure">An optional action that configures the InfoBar properties before showing it.</param>
        /// <returns>A task that completes when the info bar closes, by itself or by being dismissed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="displayDuration"/> is zero, negative, or longer than
        /// <see cref="int.MaxValue"/> milliseconds. The info bar is left untouched.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown if no InfoBar host has been registered.</exception>
        public Task ShowAsync(TimeSpan displayDuration, Action<InfoBar>? configure = null)
        {
            if (!InfoBar.IsValidDisplayDuration(displayDuration))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(displayDuration),
                    displayDuration,
                    "The display duration must be greater than zero and at most Int32.MaxValue milliseconds.");
            }

            return service.ShowAsync(infoBar =>
            {
                configure?.Invoke(infoBar);
                infoBar.DisplayDuration = displayDuration;
            });
        }
    }
}
