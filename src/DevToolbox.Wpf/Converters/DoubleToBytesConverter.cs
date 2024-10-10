using System;
using System.Globalization;
using System.Windows.Data;

namespace DevToolbox.Wpf.Converters;

internal class DoubleToBytesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is double doubleValue ? doubleValue / 2.55 : value is byte byteValue ? byteValue / 2.55 : Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is double doubleValue ? System.Convert.ToByte(doubleValue * 2.55) : Binding.DoNothing;
    }
}
