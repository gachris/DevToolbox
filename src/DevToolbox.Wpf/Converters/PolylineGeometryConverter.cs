using System.Globalization;
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace DevToolbox.Wpf.Converters;


/// <summary>
/// Converts a series of points into a closed polyline path geometry.
/// Implements <see cref="MarkupExtension"/> to allow XAML binding and <see cref="IValueConverter"/> for value conversion.
/// </summary>
public sealed class PolylineGeometryConverter : MarkupExtension, IValueConverter
{
    private readonly PathGeometry pathGeometry;
    private readonly PathFigure pathFigure;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolylineGeometryConverter"/> class.
    /// Sets up the path geometry and figure for drawing a closed polyline.
    /// </summary>
    public PolylineGeometryConverter()
    {
        pathGeometry = new PathGeometry();
        pathFigure = new PathFigure
        {
            IsClosed = true
        };
        pathGeometry.Figures.Add(pathFigure);
    }

    /// <summary>
    /// Provides the current instance of <see cref="PolylineGeometryConverter"/> for use in XAML binding.
    /// </summary>
    /// <param name="serviceProvider">The service provider that can supply services for markup extensions.</param>
    /// <returns>The current instance of <see cref="PolylineGeometryConverter"/>.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    #region IValueConverter Members

    /// <summary>
    /// Converts a <see cref="Point"/> into a <see cref="PathGeometry"/> representing a polyline.
    /// Adds the point to the path's segments or clears the path if invalid values are encountered.
    /// </summary>
    /// <param name="value">The point to be converted.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="culture">The culture information (unused).</param>
    /// <returns>The resulting <see cref="PathGeometry"/> representing the polyline.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var p = (Point)value;

        if (p.X != double.NegativeInfinity && p.Y != double.NegativeInfinity)
        {
            var lineSegment = new LineSegment(p, false);

            if (pathFigure.Segments.Count == 0)
            {
                pathFigure.StartPoint = p;
            }

            pathFigure.Segments.Add(lineSegment);
        }
        else
        {
            pathFigure.Segments.Clear();
        }

        return pathGeometry;
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