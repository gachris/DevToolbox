using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace DevToolbox.Wpf.Media;

/// <summary>
/// Manages font sizes for the WPF application based on the system's text scaling factor.
/// It listens to system-wide preference changes and updates font sizes accordingly.
/// </summary>
public class FontSizeManager
{
    #region Fields/Consts

    // Constants for text scale registry path and default text scale factor
    private const string AccessibilityRegistryPath = @"Software\Microsoft\Accessibility";
    private const string TextScaleFactorKey = "TextScaleFactor";
    private const int DefaultTextScaleFactor = 100; // Default scale (no scaling)

    // Font size range
    private const int MinFontSize = 9;
    private const int MaxFontSize = 42;

    // Font size resource key pattern
    private const string FontSizeResourceKeyFormat = "FontSize{0}";

    // Event to notify when font size has changed
    /// <summary>
    /// Occurs when the font size has been updated due to a system preference change or manual trigger.
    /// </summary>
    public static event EventHandler? FontSizeChanged;

    #endregion

    #region Properties

    private static bool _textScaleEnabled = true;

    /// <summary>
    /// Gets or sets a value indicating whether text scaling is enabled.
    /// When changed, it triggers an update of the font sizes if enabled.
    /// </summary>
    public static bool TextScaleEnabled
    {
        get => _textScaleEnabled;
        set
        {
            if (_textScaleEnabled == value) return;

            _textScaleEnabled = value;
            if (_textScaleEnabled)
            {
                UpdateFontSizes(); // Apply font size changes when enabled
            }
        }
    }

    #endregion

    /// <summary>
    /// Static constructor to subscribe to system-wide user preference changes and
    /// trigger initial font size updates based on the current settings.
    /// </summary>
    static FontSizeManager()
    {
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

        UpdateFontSizes();
    }

    #region Methods

    /// <summary>
    /// Updates the font sizes based on the text scaling factor retrieved from the system registry.
    /// </summary>
    private static void UpdateFontSizes()
    {
        if (!TextScaleEnabled) return;

        try
        {
            // Retrieve the text scaling factor from the registry
            var scalingFactor = GetTextScalingFactor();

            // Update font sizes in the application resources
            UpdateApplicationFontSizes(scalingFactor);

            // Raise the FontSizeChanged event to notify subscribers
            FontSizeChanged?.Invoke(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error while updating font sizes", ex);
        }
    }

    /// <summary>
    /// Retrieves the text scaling factor from the system registry.
    /// If the scaling factor is not found or an error occurs, the default scaling factor is returned.
    /// </summary>
    /// <returns>The text scaling factor from the system registry or the default if not found.</returns>
    private static int GetTextScalingFactor()
    {
        try
        {
            using var accessibilityKey = Registry.CurrentUser.OpenSubKey(AccessibilityRegistryPath);
            if (accessibilityKey?.GetValue(TextScaleFactorKey) is int scaleFactor)
            {
                return scaleFactor; // Return the actual scale factor if found
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error accessing registry for text scaling factor", ex);
        }

        // Return default scaling factor if not found or in case of an error
        return DefaultTextScaleFactor;
    }

    /// <summary>
    /// Updates the font sizes in the application's resource dictionary based on the provided scaling factor.
    /// </summary>
    /// <param name="scalingFactor">The scaling factor to apply to the font sizes.</param>
    private static void UpdateApplicationFontSizes(int scalingFactor)
    {
        for (var i = MinFontSize; i <= MaxFontSize; i++)
        {
            var fontSizeKey = string.Format(FontSizeResourceKeyFormat, i);

            // Calculate the scaled font size
            var scaledFontSize = i * (scalingFactor / 100.0);

            // If the resource exists, update it. If not, add it to the resources.
            if (Application.Current.Resources.Contains(fontSizeKey))
            {
                Application.Current.Resources[fontSizeKey] = scaledFontSize;
            }
            else
            {
                Application.Current.Resources.Add(fontSizeKey, scaledFontSize);
            }
        }
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles system-wide user preference changes, such as text scaling or general accessibility settings.
    /// When a relevant change is detected, it triggers a font size update.
    /// </summary>
    /// <param name="sender">The sender of the event (system).</param>
    /// <param name="e">The user preference change event arguments.</param>
    private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.General or UserPreferenceCategory.Accessibility)
        {
            Application.Current.Dispatcher.BeginInvoke(UpdateFontSizes, DispatcherPriority.Background);
        }
    }

    #endregion
}