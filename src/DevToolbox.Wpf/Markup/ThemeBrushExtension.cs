using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using DevToolbox.Wpf.Media;

namespace DevToolbox.Wpf.Markup;

/// <summary>
/// Markup extension that returns a <see cref="SolidColorBrush"/> based on the current application theme.
/// Supports high contrast and automatically updates on theme changes.
/// </summary>
[MarkupExtensionReturnType(typeof(Brush))]
public class ThemeBrushExtension : MarkupExtension, INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when the system theme or contrast mode changes.
    /// Used to trigger brush re-binding.
    /// </summary>
    public static event Action? SystemAccentChanged;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The brush to use in dark theme.
    /// </summary>
    [ConstructorArgument("darkThemeBrush")]
    public Brush DarkThemeBrush { get; set; } = null!;

    /// <summary>
    /// The brush to use in light theme.
    /// </summary>
    [ConstructorArgument("lightThemeBrush")]
    public Brush LightThemeBrush { get; set; } = null!;

    /// <summary>
    /// The fallback/default brush if the theme cannot be determined.
    /// </summary>
    [ConstructorArgument("defaultBrush")]
    public Brush DefaultBrush { get; set; } = null!;

    /// <summary>
    /// The brush to use when high contrast mode is enabled.
    /// </summary>
    [ConstructorArgument("highContrastBrush")]
    public Brush HighContrastBrush { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeBrushExtension"/> class.
    /// </summary>
    public ThemeBrushExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeBrushExtension"/> class with brushes for each mode.
    /// </summary>
    public ThemeBrushExtension(Brush darkThemeBrush, Brush lightThemeBrush, Brush defaultBrush, Brush highContrastBrush)
    {
        DarkThemeBrush = darkThemeBrush;
        LightThemeBrush = lightThemeBrush;
        DefaultBrush = defaultBrush;
        HighContrastBrush = highContrastBrush;
    }

    /// <summary>
    /// Gets the current theme-appropriate brush.
    /// </summary>
    public Brush Value
    {
        get
        {
            if (SystemParameters.HighContrast)
                return HighContrastBrush ?? DefaultBrush;

            return ThemeManager.ApplicationTheme switch
            {
                ApplicationTheme.Dark => DarkThemeBrush ?? DefaultBrush,
                ApplicationTheme.Light => LightThemeBrush ?? DefaultBrush,
                _ => DefaultBrush
            };
        }
    }

    static ThemeBrushExtension()
    {
        ThemeManager.ApplicationThemeCoreChanged += (_, _) =>
            SystemAccentChanged?.Invoke();
    }

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        SystemAccentChanged += OnSystemAccentChanged;

        return new Binding(nameof(Value))
        {
            Source = this,
            Mode = BindingMode.OneWay
        }.ProvideValue(serviceProvider);
    }

    private void OnSystemAccentChanged() =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
}