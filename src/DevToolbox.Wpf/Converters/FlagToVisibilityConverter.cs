using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// Converts a [Flags] enum to <see cref="Visibility.Visible"/> if a specific flag is set;
/// otherwise returns <see cref="Visibility.Collapsed"/>.
/// </summary>
public class FlagToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a [Flags] enum value to a <see cref="Visibility"/> value based on a specified flag.
    /// </summary>
    /// <param name="value">An <see cref="Enum"/> instance with the [Flags] attribute.</param>
    /// <param name="targetType">The type of the binding target property (unused).</param>
    /// <param name="parameter">
    /// Either the name of a flag (as a <see cref="string"/>) or an <see cref="Enum"/> value
    /// of the same type as <paramref name="value"/>.
    /// </param>
    /// <param name="culture">The culture to use in the converter (unused).</param>
    /// <returns>
    /// <see cref="Visibility.Visible"/> if <paramref name="value"/> has the specified flag;
    /// otherwise <see cref="Visibility.Collapsed"/>.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // only Enums are supported
        if (value is not Enum enumValue)
            return Visibility.Collapsed;

        Enum flagToTest;

        // parameter as string name of flag
        if (parameter is string flagName)
        {
            try
            {
                flagToTest = (Enum)Enum.Parse(enumValue.GetType(), flagName, ignoreCase: true);
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }
        // parameter already an Enum instance
        else if (parameter is Enum enumParam && enumParam.GetType() == enumValue.GetType())
        {
            flagToTest = enumParam;
        }
        else
        {
            return Visibility.Collapsed;
        }

        // test bitwise flag
        return enumValue.HasFlag(flagToTest)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    /// <summary>
    /// Not implemented: converting back from <see cref="Visibility"/> to an enum flag.
    /// </summary>
    /// <param name="value">The <see cref="Visibility"/> value (unused).</param>
    /// <param name="targetType">The type to convert to (unused).</param>
    /// <param name="parameter">The converter parameter (unused).</param>
    /// <param name="culture">The culture to use in the converter (unused).</param>
    /// <returns>Throws <see cref="NotImplementedException"/> in all cases.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
