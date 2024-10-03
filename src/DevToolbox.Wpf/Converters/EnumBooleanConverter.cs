using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// Converts an enum value to a boolean indicating whether it matches a specified parameter.
/// </summary>
public class EnumBooleanConverter : IValueConverter
{
    /// <summary>
    /// Converts the enum value to a boolean based on whether it matches the parameter.
    /// </summary>
    /// <param name="value">The enum value to convert.</param>
    /// <param name="targetType">The target type of the conversion (should be boolean).</param>
    /// <param name="parameter">The enum value to compare against, passed as a string.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>
    /// True if the enum value matches the parameter; otherwise, false. 
    /// Returns <see cref="DependencyProperty.UnsetValue"/> if input is invalid.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null) return DependencyProperty.UnsetValue;

        if (parameter is not string parameterString)
            return DependencyProperty.UnsetValue;

        if (Enum.IsDefined(value.GetType(), value) == false)
            return DependencyProperty.UnsetValue;

        var parameterValue = Enum.Parse(value.GetType(), parameterString);

        return parameterValue.Equals(value);
    }

    /// <summary>
    /// Converts the boolean value back to the corresponding enum value.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The target type of the conversion (should be an enum or nullable enum).</param>
    /// <param name="parameter">The enum value to return when the boolean is true, passed as a string.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>
    /// The corresponding enum value if the boolean is true; otherwise, null if the boolean is false.
    /// Throws an exception for invalid input.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string parameterString) return DependencyProperty.UnsetValue;

        if (targetType.IsEnum)
            return Enum.Parse(targetType, parameterString);

        Type? nullableType = Nullable.GetUnderlyingType(targetType);

        return nullableType is null
            ? throw new ArgumentException($"Provided type {targetType.Name} must be either an enum or a nullable enum")
            : Enum.Parse(nullableType, parameterString);
    }
}
