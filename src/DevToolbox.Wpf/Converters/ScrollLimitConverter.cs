using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// A converter that checks if scrolling is within limits based on Dock direction.
/// </summary>
public class ScrollLimitConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts the scroll values to a boolean indicating whether scrolling is allowed.
    /// </summary>
    /// <param name="values">An array of values where the first item is the current scroll position and the second item is the maximum scroll position.</param>
    /// <param name="targetType">The target type of the conversion.</param>
    /// <param name="parameter">The Dock direction (Left, Right, Top, Bottom).</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>
    /// Returns true if scrolling is allowed based on the direction; otherwise, returns Binding.DoNothing.
    /// </returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => (Dock?)parameter switch
    {
        Dock.Left => (double)values[0] > 0,
        Dock.Right => (double)values[0] < (double)values[1],
        Dock.Top => (double)values[0] > 0,
        Dock.Bottom => (double)values[0] < (double)values[1],
        _ => Binding.DoNothing,
    };

    /// <summary>
    /// Not implemented, as this converter does not support two-way binding.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target types for conversion.</param>
    /// <param name="parameter">The parameter for conversion.</param>
    /// <param name="culture">The culture information for conversion.</param>
    /// <returns>Throws NotImplementedException.</returns>
    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}