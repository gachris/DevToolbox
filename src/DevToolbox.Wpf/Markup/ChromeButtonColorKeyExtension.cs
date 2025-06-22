using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using DevToolbox.Wpf.Helpers;
using DevToolbox.Wpf.Interop;
using DevToolbox.Wpf.Media;
using Microsoft.Win32;
#if NET8_0_OR_GREATER && WINDOWS10_0_17763_0_OR_GREATER
using Windows.UI.ViewManagement;
#endif

namespace DevToolbox.Wpf.Markup;

/// <summary>
/// Markup extension that returns a <see cref="SolidColorBrush"/> based on the current system theme
/// and a specified <see cref="ButtonChromeBrushKey"/>.
/// Automatically updates when the system accent color changes.
/// </summary>
[MarkupExtensionReturnType(typeof(Brush))]
public class ChromeButtonColorKeyExtension : MarkupExtension, INotifyPropertyChanged
{
    private static bool _showAccentColorOnTitleBarsAndWindows;

    /// <summary>
    /// Global event triggered when the system accent color changes (e.g., user changes theme or accent).
    /// Used to notify all brush bindings to refresh.
    /// </summary>
    public static event Action? SystemAccentChanged;

    /// <summary>
    /// Occurs when the <see cref="Value"/> property changes and WPF should re-bind.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the color to use in dark theme mode.
    /// </summary>
    [ConstructorArgument("darkColor")]
    public Color DarkColor { get; set; }

    /// <summary>
    /// Gets or sets the color to use in light theme mode.
    /// </summary>
    [ConstructorArgument("lightColor")]
    public Color LightColor { get; set; }

    /// <summary>
    /// Gets or sets the optional system accent color to use, if accent color visibility is enabled.
    /// </summary>
    [ConstructorArgument("systemColorType")]
    public UIColorType? SystemColorType { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeButtonColorKeyExtension"/> class.
    /// </summary>
    public ChromeButtonColorKeyExtension()
    {
        _showAccentColorOnTitleBarsAndWindows = ActiveUserThemeReader.ShowAccentColorOnTitleBarsAndWindows();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeButtonColorKeyExtension"/> class with specified color values.
    /// </summary>
    /// <param name="darkColor">The color to use when in dark theme.</param>
    /// <param name="lightColor">The color to use when in light theme.</param>
    /// <param name="systemColorType">Optional system color to use when system accent color is enabled.</param>
    public ChromeButtonColorKeyExtension(Color darkColor, Color lightColor, UIColorType? systemColorType) : base()
    {
        DarkColor = darkColor;
        LightColor = lightColor;
        SystemColorType = systemColorType;
    }

    /// <summary>
    /// Gets the <see cref="Color"/> value based on system accent color settings and application theme.
    /// </summary>
    public Color Value => _showAccentColorOnTitleBarsAndWindows && SystemColorType.HasValue
        ? AccentColorHelper.GetColor(SystemColorType!.Value)
        : ThemeManager.ApplicationTheme is ApplicationTheme.Dark ? DarkColor : LightColor;

    static ChromeButtonColorKeyExtension()
    {
        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            if (e.Category == UserPreferenceCategory.General)
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
    /// Provides the value of the binding for this markup extension at runtime.
    /// </summary>
    /// <param name="serviceProvider">Service provider for the markup extension.</param>
    /// <returns>The result of the binding to the <see cref="Value"/> property.</returns>
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
    /// Called when the system accent changes and triggers WPF to update the binding.
    /// </summary>
    private void OnSystemAccentChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }
}
