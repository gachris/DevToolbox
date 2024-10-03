using System;
using System.Globalization;
using System.Windows.Data;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// A converter that inverts a boolean value.
/// </summary>
[ValueConversion(typeof(bool), typeof(bool))]
public class InvertBoolConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to its inverse.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type of the conversion (should be boolean).</param>
    /// <param name="parameter">An optional parameter (not used).</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>
    /// The inverted boolean value if the input is valid; otherwise, returns the original value.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value; // return original value if it's not a boolean
    }

    /// <summary>
    /// Converts the boolean value back to its inverse.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type of the conversion (should be boolean).</param>
    /// <param name="parameter">An optional parameter (not used).</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>
    /// The inverted boolean value if the input is valid; otherwise, returns the original value.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value; // return original value if it's not a boolean
    }
}
