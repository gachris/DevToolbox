using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace DevToolbox.WinUI.Contracts;

/// <summary>
/// Defines methods and properties for selecting and applying the application theme.
/// </summary>
public interface IThemeSelectorService
{
    /// <summary>
    /// Gets the current <see cref="ElementTheme"/> of the application.
    /// </summary>
    ElementTheme Theme { get; }

    /// <summary>
    /// Initializes the theme selector service, loading any persisted theme settings.
    /// </summary>
    /// <returns>
    /// A task that completes once initialization is finished.
    /// </returns>
    Task InitializeAsync();

    /// <summary>
    /// Persists and applies the specified <see cref="ElementTheme"/>.
    /// </summary>
    /// <param name="theme">
    /// The theme to apply and save (Default, Light, or Dark).
    /// </param>
    /// <returns>
    /// A task that completes when the theme has been applied.
    /// </returns>
    Task SetThemeAsync(ElementTheme theme);

    /// <summary>
    /// Applies the currently requested theme to the application window or UI root.
    /// </summary>
    /// <returns>
    /// A task that completes once the theme change has been propagated.
    /// </returns>
    Task SetRequestedThemeAsync();
}