using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// A value converter that determines the appropriate contrasting color (black or white)
/// based on the brightness of the input color.
/// </summary>
[ValueConversion(typeof(Color), typeof(Color))]
public class ContrastColorByBrightnessConverter : IValueConverter
{
    #region IValueConverter Members        

    /// <summary>
    /// Converts a <see cref="Color"/> or <see cref="SolidColorBrush"/> to a contrasting color (black or white).
    /// </summary>
    /// <param name="value">The input color or brush.</param>
    /// <param name="targetType">The target type of the conversion (should be <see cref="Color"/>).</param>
    /// <param name="parameter">An optional parameter that can be used in the conversion.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>
    /// A <see cref="Brush"/> that is black or white based on the brightness of the input color.
    /// Returns <see cref="Binding.DoNothing"/> for unsupported input types.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            Color color => Brightness(color) > 0.6 ? Brushes.Black : Brushes.White,
            SolidColorBrush solidColorBrush => Brightness(solidColorBrush.Color) > 0.6 ? Brushes.Black : Brushes.White,
            _ => Binding.DoNothing,
        };
    }

    /// <summary>
    /// Not implemented for this converter as it only performs one-way conversion.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

    /// <summary>
    /// Calculates the brightness of the given color.
    /// </summary>
    /// <param name="color">The color to calculate brightness for.</param>
    /// <returns>A double representing the brightness of the color, ranging from 0.0 to 1.0.</returns>
    private static double Brightness(Color color) => (double)Math.Max(Math.Max(color.R, color.G), color.B) / 255;

    #endregion
}
