using System;
using System.Globalization;
using System.Windows.Data;

namespace DevToolbox.Wpf.Converters;

internal class HexColorCharacterHidingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hexColor)
        {
            if (hexColor.StartsWith("#"))
            {
                return hexColor[1..].ToLower();
            }
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hexColor)
        {
            if (!hexColor.StartsWith("#"))
            {
                return "#" + hexColor;
            }
        }

        return value;
    }
}