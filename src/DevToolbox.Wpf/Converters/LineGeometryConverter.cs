using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// Converts a <see cref="Point"/> into a <see cref="GeometryGroup"/> that contains 
/// dashed lines representing horizontal and vertical axes through the given point.
/// Implements <see cref="MarkupExtension"/> for XAML support and <see cref="IValueConverter"/> for value conversion.
/// </summary>
public sealed class LineGeometryConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Provides the current instance of <see cref="LineGeometryConverter"/> for use in XAML binding.
    /// </summary>
    /// <param name="serviceProvider">The service provider that can supply services for markup extensions.</param>
    /// <returns>The current instance of <see cref="LineGeometryConverter"/>.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    #region IValueConverter Members

    /// <summary>
    /// Converts a <see cref="Point"/> to a <see cref="GeometryGroup"/> containing 
    /// vertical and horizontal lines through the specified point.
    /// </summary>
    /// <param name="value">The <see cref="Point"/> used to define the line intersection.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="culture">The culture information (unused).</param>
    /// <returns>A <see cref="GeometryGroup"/> with dashed lines drawn through the specified point.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var p = (Point)value; // The input point

        var group = new GeometryGroup();

        // Create vertical and horizontal lines
        group.Children.Add(new LineGeometry(new Point(p.X, 0), new Point(p.X, 10000))); // Vertical line
        group.Children.Add(new LineGeometry(new Point(0, p.Y), new Point(10000, p.Y))); // Horizontal line

        return group; // Return the geometry group
    }

    /// <summary>
    /// ConvertBack is not implemented and will throw a <see cref="NotImplementedException"/> if called.
    /// </summary>
    /// <param name="value">The value to convert back (unused).</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="culture">The culture information (unused).</param>
    /// <returns>Throws a <see cref="NotImplementedException"/>.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}
