using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using DevToolbox.Wpf.Interop;
using Microsoft.Win32;

namespace DevToolbox.Wpf.Media;

/// <summary>
/// Provides functionality to manage application themes within a WPF application.
/// This class allows setting and retrieving the current theme, managing the high contrast mode, 
/// and notifying subscribers when the theme is changed.
/// </summary>
public static class ThemeManager
{
    #region Fields/Constants

    // Current theme-related fields
    private static ElementTheme _requestedTheme = ElementTheme.Default;
    private static ApplicationTheme _applicationTheme = ApplicationTheme.Default;

    /// <summary>
    /// Occurs when the requested theme is changed.
    /// Subscribers can listen for this event to respond to theme changes.
    /// </summary>
    public static event EventHandler? RequestedThemeChanged;

    /// <summary>
    /// Occurs when the application theme is changed.
    /// Subscribers can listen for this event to respond to theme changes.
    /// </summary>
    public static event EventHandler? ApplicationThemeChanged;

    /// <summary>
    /// Internal event raised when the application theme changes due to system-level changes (e.g., high contrast mode).
    /// </summary>
    internal static event EventHandler? ApplicationThemeCoreChanged;

    #endregion

    // Static constructor to set up event subscriptions
    static ThemeManager()
    {
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
    }

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether the application should override core theming.
    /// When set to <c>true</c>, theme resource dictionaries originating from the DevToolbox assemblies
    /// will be ignored during theme application. This allows application developers to fully customize
    /// or replace the default DevToolbox-provided themes.
    /// </summary>
    /// <remarks>
    /// Use this flag to prevent automatic loading of DevToolbox theme resources, especially when you want
    /// to implement a custom theming system or fully control the visual appearance of the application
    /// without interference from predefined styles.
    /// </remarks>
    public static bool OverrideCoreTheming { get; set; }

    /// <summary>
    /// Gets or sets the current requested theme.
    /// Changing this property triggers theme updates and raises the appropriate events.
    /// </summary>
    public static ElementTheme RequestedTheme
    {
        get => _requestedTheme;
        set
        {
            if (_requestedTheme != value)
            {
                var oldTheme = _requestedTheme;
                _requestedTheme = value;

                RefreshTheme(oldValue: oldTheme, newValue: _requestedTheme);

                // Notify subscribers that the requested theme has changed
                RequestedThemeChanged?.Invoke(Application.Current, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets the current application theme that is actively applied to the application.
    /// This reflects the final theme state after considering user preferences and system defaults.
    /// </summary>
    public static ApplicationTheme ApplicationTheme => _applicationTheme;

    /// <summary>
    /// Returns the current application theme's name as a string.
    /// Useful for UI bindings or diagnostics.
    /// </summary>
    internal static string ApplicationThemeName => _applicationTheme.ToString();

    /// <summary>
    /// Gets the application theme name, considering high contrast mode.
    /// If high contrast mode is enabled, this will return "HighContrast".
    /// </summary>
    internal static string ApplicationThemeNameCore => !SystemParameters.HighContrast ? ApplicationThemeName : "HighContrast";

    #endregion

    #region Methods

    /// <summary>
    /// Refreshes the application theme by applying the new requested theme and raising necessary events.
    /// </summary>
    /// <param name="oldValue">The previously set theme.</param>
    /// <param name="newValue">The new theme to apply.</param>
    private static void RefreshTheme(ElementTheme oldValue, ElementTheme newValue)
    {
        // Determine the new application theme based on the requested theme
        var newAppTheme = newValue switch
        {
            ElementTheme.Default => ApplicationTheme.Default,
            ElementTheme.Dark => ApplicationTheme.Dark,
            ElementTheme.Light => ApplicationTheme.Light,
            ElementTheme.WindowsDefault => GetWindowsTheme(),
            _ => throw new ArgumentOutOfRangeException(nameof(newValue)),
        };

        // If the application theme has changed, notify subscribers and update the theme
        if (_applicationTheme != newAppTheme)
        {
            _applicationTheme = newAppTheme;
            ApplicationThemeChanged?.Invoke(Application.Current, EventArgs.Empty);
            ApplicationThemeCoreChanged?.Invoke(Application.Current, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Retrieves the current Windows theme setting (either Light or Dark) from the system registry.
    /// </summary>
    /// <returns>The application theme based on the current Windows system setting.</returns>
    private static ApplicationTheme GetWindowsTheme()
    {
        try
        {
            return ActiveUserThemeReader.GetThemeForActiveUser();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error accessing registry for Windows theme settings: {ex.Message}");
        }

        // Default to Dark theme if no theme is found or an error occurs
        return ApplicationTheme.Dark;
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles system-wide user preference changes.
    /// This method responds to changes such as high contrast mode or general system preferences and updates the theme accordingly.
    /// </summary>
    /// <param name="sender">The sender of the event (system).</param>
    /// <param name="e">The user preference change event arguments.</param>
    private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            // Respond to high contrast mode changes
            if (e.Category == UserPreferenceCategory.Accessibility)
            {
                ApplicationThemeCoreChanged?.Invoke(Application.Current, EventArgs.Empty);
            }

            // Respond to general changes when the theme is set to follow the Windows default theme
            if (e.Category == UserPreferenceCategory.General && RequestedTheme == ElementTheme.WindowsDefault && !SystemParameters.HighContrast)
            {
                var windowsTheme = GetWindowsTheme();
                if (_applicationTheme != windowsTheme)
                {
                    _applicationTheme = windowsTheme;
                    ApplicationThemeChanged?.Invoke(Application.Current, EventArgs.Empty);
                    ApplicationThemeCoreChanged?.Invoke(Application.Current, EventArgs.Empty);
                }
            }
        }, DispatcherPriority.Background);
    }

    #endregion
}
