using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// Converts an array of two points into an <see cref="EllipseGeometry"/> for use in XAML.
/// Implements <see cref="MarkupExtension"/> for XAML support and <see cref="IMultiValueConverter"/> for multi-value conversion.
/// </summary>
public sealed class EllipseGeometryConverter : MarkupExtension, IMultiValueConverter
{
    /// <summary>
    /// Provides the current instance of <see cref="EllipseGeometryConverter"/> for use in XAML binding.
    /// </summary>
    /// <param name="serviceProvider">The service provider that can supply services for markup extensions.</param>
    /// <returns>The current instance of <see cref="EllipseGeometryConverter"/>.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    #region IMultiValueConverter Members

    /// <summary>
    /// Converts an array of two <see cref="Point"/> values into an <see cref="EllipseGeometry"/>.
    /// The first point represents the center, and the second point represents the size of the ellipse.
    /// </summary>
    /// <param name="values">An array containing two <see cref="Point"/> objects.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="culture">The culture information (unused).</param>
    /// <returns>A new <see cref="EllipseGeometry"/> based on the provided points.</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return new EllipseGeometry(new Rect((Point)values[0], (Point)values[1]));
    }

    /// <summary>
    /// ConvertBack is not implemented and will throw a <see cref="NotImplementedException"/> if called.
    /// </summary>
    /// <param name="value">The value to convert back (unused).</param>
    /// <param name="targetTypes">The target types (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="culture">The culture information (unused).</param>
    /// <returns>Throws a <see cref="NotImplementedException"/>.</returns>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    #endregion
}
