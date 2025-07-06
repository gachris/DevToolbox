using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// Converts a [Flags] enum to Visibility.Visible if a specific flag is set; otherwise Visibility.Collapsed.
/// </summary>
public class FlagToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// <para>
    /// Expects:
    ///  - value: any Enum instance with [Flags] attribute.
    ///  - parameter: either the name of a flag (string) or an Enum value of the same type as value.
    /// </para>
    /// <para>
    /// Returns Visible if <c>value.HasFlag(parameter)</c>, otherwise Collapsed.
    /// </para>
    /// </summary>
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}