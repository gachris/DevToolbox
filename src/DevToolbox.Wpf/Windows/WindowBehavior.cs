using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DevToolbox.Wpf.Interop;
using DevToolbox.Wpf.Windows.Effects;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// Provides attached properties and behaviors for customizing window effects
/// and border colors in WPF applications.
/// </summary>
public class WindowBehavior
{
    #region Fields/Consts

    /// <summary>
    /// Attached Dependency Property for setting the window effect.
    /// </summary>
    public static readonly DependencyProperty EffectProperty =
        DependencyProperty.RegisterAttached("Effect", typeof(Effect), typeof(WindowBehavior), new PropertyMetadata(null, OnWindowEffectChanged));

    /// <summary>
    /// Read-only Dependency Property Key for indicating if a window has an effect applied.
    /// </summary>
    public static readonly DependencyPropertyKey HasEffectPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly("HasEffect", typeof(bool), typeof(WindowBehavior), new PropertyMetadata(false));

    /// <summary>
    /// Read-only Dependency Property indicating whether the window has an effect applied.
    /// </summary>
    public static readonly DependencyProperty HasEffectProperty = HasEffectPropertyKey.DependencyProperty;

    /// <summary>
    /// Attached Dependency Property for setting the border brush of the window.
    /// </summary>
    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.RegisterAttached("BorderBrush", typeof(Brush), typeof(WindowBehavior), new FrameworkPropertyMetadata(Brushes.Transparent, OnBorderBrushChanged));

    #endregion

    #region Methods

    /// <summary>
    /// Gets the window effect attached to the specified window.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <returns>The window effect applied to the window, if any.</returns>
    public static Effect? GetWindowEffect(Window window)
    {
        return (Effect?)window.GetValue(EffectProperty);
    }

    /// <summary>
    /// Sets the window effect attached to the specified window.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="windowEffect">The effect to apply to the window.</param>
    public static void SetWindowEffect(Window window, Effect? windowEffect)
    {
        window.SetValue(EffectProperty, windowEffect);
    }

    /// <summary>
    /// Gets a value indicating whether the specified window has an effect applied.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <returns>True if the window has an effect; otherwise, false.</returns>
    public static bool GetHasEffect(Window window)
    {
        return (bool)window.GetValue(HasEffectProperty);
    }

    /// <summary>
    /// Sets the border brush of the specified window.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="brush">The brush to use for the window border.</param>
    public static void SetBorderBrush(Window window, Brush brush) => window.SetValue(BorderBrushProperty, brush);

    /// <summary>
    /// Gets the border brush of the specified window.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <returns>The brush used for the window border.</returns>
    public static Brush GetBorderBrush(Window window) => (Brush)window.GetValue(BorderBrushProperty);

    /// <summary>
    /// Callback method invoked when the Effect property changes.
    /// Applies or removes the specified effect from the window.
    /// </summary>
    /// <param name="d">The target DependencyObject.</param>
    /// <param name="e">The event data for the property change.</param>
    private static void OnWindowEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = (Window)d;

        if (!IsSupported())
        {
            // Revert to previous value or remove the effect
            window.ClearValue(EffectProperty);
            window.SetValue(HasEffectPropertyKey, false);
            return;
        }

        var oldEffect = e.OldValue as Effect;
        var newEffect = e.NewValue as Effect;

        oldEffect?.Detach();
        newEffect?.Attach(window);

        window.SetValue(HasEffectPropertyKey, newEffect is not null);
    }

    /// <summary>
    /// Callback method invoked when the BorderBrush property changes.
    /// Updates the window's border color accordingly.
    /// </summary>
    /// <param name="d">The target DependencyObject.</param>
    /// <param name="e">The event data for the property change.</param>
    private static void OnBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window) return;

        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd.Equals(IntPtr.Zero))
        {
            window.SourceInitialized += (_, _) =>
            {
                hwnd = new WindowInteropHelper(window).Handle;
                if (!hwnd.Equals(IntPtr.Zero))
                {
                    var brush = (SolidColorBrush?)e.NewValue;
                    SetWindowBorderColor(hwnd, brush?.Color);
                }
            };
        }
        else
        {
            var brush = (SolidColorBrush?)e.NewValue;
            SetWindowBorderColor(hwnd, brush?.Color);
        }
    }

    /// <summary>
    /// Sets the border color of the specified window handle.
    /// </summary>
    /// <param name="hwnd">The window handle.</param>
    /// <param name="color">The color to set as the border.</param>
    private static void SetWindowBorderColor(IntPtr hwnd, Color? color)
    {
        var pvattribute = color.HasValue ? (uint)NativeMethods.GetDWORD(color.Value) : 0x00;
        _ = Dwmapi.DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_BORDER_COLOR, ref pvattribute, sizeof(uint));
    }

    /// <summary>
    /// Determines if the current OS version supports the window effect features.
    /// </summary>
    /// <returns>True if supported; otherwise, false.</returns>
    private static bool IsSupported()
    {
        var v = NativeMethods.GetTrueOSVersion();
        return v.Build >= 22621;
    }

    #endregion
}