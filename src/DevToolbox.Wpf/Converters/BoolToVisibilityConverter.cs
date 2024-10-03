using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// A value converter that converts a boolean value to a <see cref="Visibility"/> enumeration value.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the <see cref="Visibility"/> value to return when the input value is <c>true</c>.
    /// Default is <see cref="Visibility.Visible"/>.
    /// </summary>
    public Visibility TrueValue { get; set; } = Visibility.Visible;

    /// <summary>
    /// Gets or sets the <see cref="Visibility"/> value to return when the input value is <c>false</c>.
    /// Default is <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public Visibility FalseValue { get; set; } = Visibility.Collapsed;

    /// <summary>
    /// Converts a boolean value to a <see cref="Visibility"/> value.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The target type of the conversion (should be <see cref="Visibility"/>).</param>
    /// <param name="parameter">An optional parameter that can be used in the conversion.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>
    /// <see cref="Visibility.Visible"/> if <paramref name="value"/> is <c>true</c>; otherwise, <see cref="Visibility.Collapsed"/>.
    /// Returns <see cref="DependencyProperty.UnsetValue"/> if the input is not a boolean.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? TrueValue : FalseValue;
        }
        return DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Converts a <see cref="Visibility"/> value back to a boolean value.
    /// </summary>
    /// <param name="value">The visibility value to convert.</param>
    /// <param name="targetType">The target type for the conversion (should be <see cref="bool"/>).</param>
    /// <param name="parameter">An optional parameter that can be used in the conversion.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> is equal to <see cref="TrueValue"/>; otherwise, <c>false</c>.
    /// Returns <see cref="DependencyProperty.UnsetValue"/> if the input is not a <see cref="Visibility"/> value.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == TrueValue;
        }
        return DependencyProperty.UnsetValue;
    }
}
