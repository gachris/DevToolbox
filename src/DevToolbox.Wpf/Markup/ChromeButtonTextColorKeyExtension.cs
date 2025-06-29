using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using DevToolbox.Wpf.Helpers;
using DevToolbox.Wpf.Interop;
using DevToolbox.Wpf.Media;
using Microsoft.Win32;

namespace DevToolbox.Wpf.Markup;

/// <summary>
/// Markup extension that returns a <see cref="SolidColorBrush"/> based on the system accent color,
/// with optional brightness adjustment depending on theme or user settings.
/// </summary>
[MarkupExtensionReturnType(typeof(Brush))]
public class ChromeButtonTextColorKeyExtension : MarkupExtension, INotifyPropertyChanged
{
    private static bool _showAccentColorOnTitleBarsAndWindows;

    /// <summary>
    /// Occurs when the system accent or theme setting changes.
    /// </summary>
    public static event Action? SystemAccentChanged;

    /// <summary>
    /// Occurs when a property value changes, enabling data binding updates.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the primary system color type to use for luminance checks.
    /// </summary>
    public UIColorType SystemColorType { get; set; }

    /// <summary>
    /// Gets or sets the color to use if the base color is considered dark.
    /// </summary>
    public UIColorType DarkColorType { get; set; }

    /// <summary>
    /// Gets or sets the color to use if the base color is considered light.
    /// </summary>
    public UIColorType LightColorType { get; set; }

    /// <summary>
    /// Gets or sets the background color used for luminance evaluation in dark theme.
    /// </summary>
    public Color DarkBackground { get; set; } = Colors.Black;

    /// <summary>
    /// Gets or sets the background color used for luminance evaluation in light theme.
    /// </summary>
    public Color LightBackground { get; set; } = Colors.White;

    /// <summary>
    /// Gets or sets the foreground color to return in dark background scenarios.
    /// </summary>
    public Color DarkForeground { get; set; } = Colors.White;

    /// <summary>
    /// Gets or sets the foreground color to return in light background scenarios.
    /// </summary>
    public Color LightForeground { get; set; } = Colors.Black;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeButtonTextColorKeyExtension"/> class.
    /// </summary>
    public ChromeButtonTextColorKeyExtension()
    {
        _showAccentColorOnTitleBarsAndWindows = ActiveUserThemeReader.ShowAccentColorOnTitleBarsAndWindows();
    }

    /// <summary>
    /// Gets the <see cref="Color"/> value based on the system accent color and perceived luminance.
    /// </summary>
    public Color Value
    {
        get
        {
            if (_showAccentColorOnTitleBarsAndWindows)
            {
                var baseColor = AccentColorHelper.GetColor(SystemColorType);
                var luminance = (0.299 * baseColor.R + 0.587 * baseColor.G + 0.114 * baseColor.B) / 255;
                return luminance > 0.5 ? AccentColorHelper.GetColor(DarkColorType) : AccentColorHelper.GetColor(LightColorType);
            }
            else
            {
                var baseColor = ThemeManager.ApplicationTheme is ApplicationTheme.Dark ? DarkBackground : LightBackground;
                var luminance = (0.299 * baseColor.R + 0.587 * baseColor.G + 0.114 * baseColor.B) / 255;
                return luminance > 0.5 ? DarkForeground : LightForeground;
            }
        }
    }

    static ChromeButtonTextColorKeyExtension()
    {
        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            if (e.Category is UserPreferenceCategory.General)
            {
                _showAccentColorOnTitleBarsAndWindows = ActiveUserThemeReader.ShowAccentColorOnTitleBarsAndWindows();
                SystemAccentChanged?.Invoke();
            }
        };

        ThemeManager.ApplicationThemeChanged += (s, e) =>
        {
            SystemAccentChanged?.Invoke();
        };
    }

    /// <summary>
    /// Returns the object that is set on the property where the markup extension is applied.
    /// </summary>
    /// <param name="serviceProvider">A service provider to obtain services for the markup extension.</param>
    /// <returns>A dynamic binding to the calculated <see cref="Color"/> based on system and theme settings.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        SystemAccentChanged += OnSystemAccentChanged;

        var binding = new Binding(nameof(Value))
        {
            Source = this,
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }

    /// <summary>
    /// Handles system or theme changes by notifying property change listeners.
    /// </summary>
    private void OnSystemAccentChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }
}