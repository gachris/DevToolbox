using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace DevToolbox.Wpf.Theming;

/// <summary>
/// Provides functionality to manage application themes within a WPF application.
/// This class allows setting and retrieving the current theme and notifies subscribers of theme changes.
/// </summary>
public static class ThemeManager
{
    #region Fields/Constants

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
    /// Occurs when the application theme is changed.
    /// Subscribers can listen for this event to respond to theme changes.
    /// </summary>
    internal static event EventHandler? ApplicationThemeCoreChanged;

    #endregion

    static ThemeManager()
    {
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        if (SystemParameters.HighContrast)
        {
            ApplicationThemeCoreChanged?.Invoke(Application.Current, EventArgs.Empty);
        }
    }

    #region Properties

    /// <summary>
    /// Gets the current requested theme.
    /// </summary>
    public static ElementTheme RequestedTheme
    {
        get => _requestedTheme;
        set
        {
            var oldValue = _requestedTheme;
            if (oldValue != value)
            {
                _requestedTheme = value;
                RefreshTheme(oldValue: oldValue, newValue: _requestedTheme);
            }
        }
    }

    /// <summary>
    /// Gets the current application theme.
    /// </summary>
    public static ApplicationTheme ApplicationTheme => _applicationTheme;

    /// <summary>
    /// Gets the current application theme.
    /// </summary>
    internal static string ApplicationThemeName => _applicationTheme.ToString();

    /// <summary>
    /// Gets the current application theme.
    /// </summary>
    internal static string ApplicationThemeNameCore => !SystemParameters.HighContrast ? ApplicationThemeName : "HighContrast";

    #endregion

    #region Methods

    /// <summary>
    /// Updates the current application theme.
    /// </summary>
    /// <param name="oldValue">The old theme.</param>
    /// <param name="newValue">The new theme to apply.</param>
    private static void RefreshTheme(ElementTheme oldValue, ElementTheme newValue)
    {
        RequestedThemeChanged?.Invoke(Application.Current, EventArgs.Empty);

        var newApplicationTheme = newValue switch
        {
            ElementTheme.Default => ApplicationTheme.Default,
            ElementTheme.Dark => ApplicationTheme.Dark,
            ElementTheme.Light => ApplicationTheme.Light,
            ElementTheme.WindowsDefault => GetWindowsTheme(),
            _ => throw new ArgumentOutOfRangeException(nameof(newValue)),
        };

        if (_applicationTheme != newApplicationTheme)
        {
            _applicationTheme = newApplicationTheme;
            ApplicationThemeChanged?.Invoke(Application.Current, EventArgs.Empty);
            ApplicationThemeCoreChanged?.Invoke(Application.Current, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets the current Windows theme (Light or Dark).
    /// </summary>
    /// <returns>The current application theme based on the Windows setting.</returns>
    private static ApplicationTheme GetWindowsTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int intValue)
            {
                return intValue == 1 ? ApplicationTheme.Light : ApplicationTheme.Dark;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error accessing registry for Windows theme settings: {ex.Message}");
        }

        return ApplicationTheme.Dark;
    }

    #endregion

    #region Events Subscriptions

    private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            if (e.Category == UserPreferenceCategory.Accessibility)
            {
                ApplicationThemeCoreChanged?.Invoke(Application.Current, EventArgs.Empty);
            }

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