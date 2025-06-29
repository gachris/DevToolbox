using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A ContentControl that provides an acrylic (tinted + noise) background effect.
/// </summary>
public class AcrylicPanel : ContentControl
{
    /// <summary>
    /// Identifies the TintBrush dependency property.
    /// </summary>
    public static readonly DependencyProperty TintBrushProperty =
        DependencyProperty.RegisterAttached(
            "TintBrush",
            typeof(Brush),
            typeof(AcrylicPanel),
            new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// Identifies the TintOpacity dependency property.
    /// </summary>
    public static readonly DependencyProperty TintOpacityProperty =
        DependencyProperty.RegisterAttached(
            "TintOpacity",
            typeof(double),
            typeof(AcrylicPanel),
            new FrameworkPropertyMetadata(0.6, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// Identifies the NoiseOpacity dependency property.
    /// </summary>
    public static readonly DependencyProperty NoiseOpacityProperty =
        DependencyProperty.RegisterAttached(
            "NoiseOpacity",
            typeof(double),
            typeof(AcrylicPanel),
            new FrameworkPropertyMetadata(0.03, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// Identifies the CornerRadius dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(AcrylicPanel),
            new PropertyMetadata(default(CornerRadius)));

    /// <summary>
    /// Gets or sets the corner radius for the panel.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets the TintBrush value for a specified element.
    /// </summary>
    /// <param name="obj">The element to retrieve the value from.</param>
    /// <returns>The <see cref="Brush"/> used as the tint.</returns>
    public static Brush GetTintBrush(DependencyObject obj)
    {
        return (Brush)obj.GetValue(TintBrushProperty);
    }

    /// <summary>
    /// Sets the TintBrush value for a specified element.
    /// </summary>
    /// <param name="obj">The element to set the value on.</param>
    /// <param name="value">The brush to use as the tint.</param>
    public static void SetTintBrush(DependencyObject obj, Brush value)
    {
        obj.SetValue(TintBrushProperty, value);
    }

    /// <summary>
    /// Gets the TintOpacity value for a specified element.
    /// </summary>
    /// <param name="obj">The element to retrieve the value from.</param>
    /// <returns>The opacity of the tint.</returns>
    public static double GetTintOpacity(DependencyObject obj)
    {
        return (double)obj.GetValue(TintOpacityProperty);
    }

    /// <summary>
    /// Sets the TintOpacity value for a specified element.
    /// </summary>
    /// <param name="obj">The element to set the value on.</param>
    /// <param name="value">The opacity of the tint.</param>
    public static void SetTintOpacity(DependencyObject obj, double value)
    {
        obj.SetValue(TintOpacityProperty, value);
    }

    /// <summary>
    /// Gets the NoiseOpacity value for a specified element.
    /// </summary>
    /// <param name="obj">The element to retrieve the value from.</param>
    /// <returns>The opacity of the noise overlay.</returns>
    public static double GetNoiseOpacity(DependencyObject obj)
    {
        return (double)obj.GetValue(NoiseOpacityProperty);
    }

    /// <summary>
    /// Sets the NoiseOpacity value for a specified element.
    /// </summary>
    /// <param name="obj">The element to set the value on.</param>
    /// <param name="value">The opacity of the noise overlay.</param>
    public static void SetNoiseOpacity(DependencyObject obj, double value)
    {
        obj.SetValue(NoiseOpacityProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to tint the background.
    /// </summary>
    public Brush TintBrush
    {
        get => (Brush)GetValue(TintBrushProperty);
        set => SetValue(TintBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the opacity of the tint.
    /// </summary>
    public double TintOpacity
    {
        get => (double)GetValue(TintOpacityProperty);
        set => SetValue(TintOpacityProperty, value);
    }

    /// <summary>
    /// Gets or sets the opacity of the noise overlay.
    /// </summary>
    public double NoiseOpacity
    {
        get => (double)GetValue(NoiseOpacityProperty);
        set => SetValue(NoiseOpacityProperty, value);
    }

    static AcrylicPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AcrylicPanel), new FrameworkPropertyMetadata(typeof(AcrylicPanel)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AcrylicPanel"/> class.
    /// </summary>
    public AcrylicPanel()
    {
    }
}
