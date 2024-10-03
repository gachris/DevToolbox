using System;
using System.Windows;

namespace DevToolbox.Wpf.Theming;

/// <summary>
/// Provides functionality to manage application themes within a WPF application.
/// This class allows setting and retrieving the current theme and notifies subscribers of theme changes.
/// </summary>
public static class ThemeManager
{
    #region Fields/Constants

    private static Theme _applicationTheme = Theme.Default; // The current theme applied to the application

    #endregion

    #region Properties

    /// <summary>
    /// Gets the current application theme.
    /// </summary>
    public static Theme ApplicationTheme => _applicationTheme;

    /// <summary>
    /// Occurs when the application theme is changed.
    /// Subscribers can listen for this event to respond to theme changes.
    /// </summary>
    public static event EventHandler? ApplicationThemeChanged;

    #endregion

    #region Methods

    /// <summary>
    /// Applies a new theme to the application.
    /// Updates the current application theme and raises the <see cref="ApplicationThemeChanged"/> event.
    /// </summary>
    /// <param name="app">The application instance to which the theme is applied.</param>
    /// <param name="newTheme">The new theme to apply.</param>
    public static void ApplyTheme(this Application app, Theme newTheme)
    {
        _applicationTheme = newTheme; // Update the application theme

        // Notify subscribers that the application theme has changed
        ApplicationThemeChanged?.Invoke(app, EventArgs.Empty);
    }

    #endregion
}