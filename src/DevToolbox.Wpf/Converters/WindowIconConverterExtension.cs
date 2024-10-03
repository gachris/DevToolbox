using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevToolbox.Wpf.Utils;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// A markup extension for converting various image sources into a suitable window icon.
/// </summary>
public class WindowIconConverterExtension : MarkupExtension, IMultiValueConverter
{
    private static WindowIconConverterExtension? _converter;

    /// <summary>
    /// Converts the provided values into an appropriate icon image.
    /// </summary>
    /// <param name="value">The values to convert.</param>
    /// <param name="targetType">The target type of the binding.</param>
    /// <param name="parameter">Optional parameter for the conversion.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>The converted image source.</returns>
    [SecuritySafeCritical]
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        var obj = value[0];

        switch (obj)
        {
            case BitmapFrame bitmapFrame:
                // Return the first frame of the bitmap that matches 16x16 size
                return bitmapFrame.Decoder.Frames.FirstOrDefault(x => x.Width == 16.0 && x.Height == 16.0) ?? bitmapFrame;

            case ImageSource imageSource:
                // Return the existing image source
                return imageSource;

            default:
                // Retrieve large and small icons using ImageSourceHelper
                var imageSourceList = new List<ImageSource>();
                ImageSourceHelper.GetIcons(out ImageSource[] largeIcons, out ImageSource[] smallIcons);

                if (largeIcons.Length == 0)
                    // Default to application icon if no icons are available
                    return ImageSourceHelper.GetImageSource(SystemIcons.Application);

                // Return the first large icon if available
                return largeIcons.First();
        }
    }

    /// <summary>
    /// Not implemented for two-way binding.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target type of the binding.</param>
    /// <param name="parameter">Optional parameter for the conversion.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>Throws NotImplementedException.</returns>
    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    /// <summary>
    /// Provides the value for the markup extension.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The instance of this converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => _converter ??= new WindowIconConverterExtension();
}
