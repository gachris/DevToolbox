using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace DevToolbox.WinUI.Converters;

/// <summary>
/// Converts an <see cref="Enum"/> value to a <see cref="bool"/> by comparing
/// it against a specified enum name, and back to an enum value from a boolean.
/// </summary>
public class EnumToBooleanConverter : IValueConverter
{
    /// <summary>
    /// Converts an enum value to a boolean by comparing it with the provided parameter.
    /// </summary>
    /// <param name="value">
    /// The enum value to convert.
    /// </param>
    /// <param name="targetType">
    /// The expected type of the conversion result (should be <see cref="bool"/>).
    /// </param>
    /// <param name="parameter">
    /// A string representing the name of the enum member to compare against.
    /// </param>
    /// <param name="language">
    /// The culture information for localization purposes (unused).
    /// </param>
    /// <returns>
    /// <c>true</c> if the enum value matches the specified parameter; otherwise, <see cref="DependencyProperty.UnsetValue"/>
    /// if the conversion cannot be performed.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is null)
            return DependencyProperty.UnsetValue;

        if (parameter is not string parameterString)
            return DependencyProperty.UnsetValue;

        var valueType = value.GetType();
        if (!Enum.IsDefined(valueType, value))
            return DependencyProperty.UnsetValue;

        var parameterValue = Enum.Parse(valueType, parameterString);
        return parameterValue.Equals(value);
    }

    /// <summary>
    /// Converts back a boolean to an enum value based on the provided parameter.
    /// </summary>
    /// <param name="value">
    /// The boolean value indicating whether to select the enum member.
    /// </param>
    /// <param name="targetType">
    /// The type to convert back to, which must be an enum or nullable enum.
    /// </param>
    /// <param name="parameter">
    /// A string representing the name of the enum member to return.
    /// </param>
    /// <param name="language">
    /// The culture information for localization purposes (unused).
    /// </param>
    /// <returns>
    /// The corresponding enum value if <paramref name="value"/> is <c>true</c>;
    /// otherwise, <see cref="DependencyProperty.UnsetValue"/> or throws if the target type is invalid.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="targetType"/> is neither an enum nor a nullable enum.
    /// </exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not string parameterString)
            return DependencyProperty.UnsetValue;

        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, parameterString);
        }

        var nullableType = Nullable.GetUnderlyingType(targetType);
        return nullableType is null
            ? throw new ArgumentException($"Provided type {targetType.Name} must be either an enum or a nullable enum")
            : Enum.Parse(nullableType, parameterString);
    }
}