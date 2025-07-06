using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// Converts a width value to a <see cref="Visibility"/> state based on a threshold.
/// </summary>
public class WidthThresholdToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a width value to a <see cref="Visibility"/> state.
    /// </summary>
    /// <param name="value">The width value to evaluate, expected to be of type <see cref="double"/>.</param>
    /// <param name="targetType">The target binding type (unused).</param>
    /// <param name="parameter">A string representing the threshold against which to compare the width.</param>
    /// <param name="culture">The culture information (unused).</param>
    /// <returns>
    /// <see cref="Visibility.Visible"/> if the width is less than the threshold; otherwise, <see cref="Visibility.Collapsed"/>.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double width && double.TryParse((string)parameter, out double threshold))
        {
            return width < threshold
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    /// <summary>
    /// Not implemented. Converts back from <see cref="Visibility"/> to width.
    /// </summary>
    /// <param name="value">The <see cref="Visibility"/> value (unused).</param>
    /// <param name="targetType">The target binding type (unused).</param>
    /// <param name="parameter">An optional parameter (unused).</param>
    /// <param name="culture">The culture information (unused).</param>
    /// <returns>Throws <see cref="NotImplementedException"/>.</returns>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}