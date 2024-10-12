using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// Converts a <see cref="Point"/> into a <see cref="ControlTemplate"/> containing a 
/// <see cref="Polyline"/> that represents a series of connected lines. 
/// Implements <see cref="MarkupExtension"/> for XAML support and <see cref="IValueConverter"/> for value conversion.
/// </summary>
public sealed class PolylineConverter : MarkupExtension, IValueConverter
{
    private readonly List<Point> points = []; // Stores points for the polyline

    /// <summary>
    /// Provides the current instance of <see cref="PolylineConverter"/> for use in XAML binding.
    /// </summary>
    /// <param name="serviceProvider">The service provider that can supply services for markup extensions.</param>
    /// <returns>The current instance of <see cref="PolylineConverter"/>.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    #region IValueConverter Members

    /// <summary>
    /// Converts a <see cref="Point"/> to a <see cref="ControlTemplate"/> containing a 
    /// <see cref="Polyline"/>. If the point has valid coordinates, it adds the point to 
    /// the list; otherwise, it clears the list of points.
    /// </summary>
    /// <param name="value">The <see cref="Point"/> used to define the polyline points.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="culture">The culture information (unused).</param>
    /// <returns>A <see cref="ControlTemplate"/> containing the <see cref="Polyline"/> with the defined points.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var p = (Point)value; // The input point

        // Check if the point is valid
        if (p.X != double.NegativeInfinity && p.Y != double.NegativeInfinity)
        {
            points.Add(p); // Add the point to the list
        }
        else
        {
            points.Clear(); // Clear the points if the point is invalid
        }

        // Create a ControlTemplate with a Polyline
        var template = new ControlTemplate(typeof(Control))
        {
            VisualTree = new FrameworkElementFactory(typeof(Polyline))
        };
        template.VisualTree.SetValue(Shape.StrokeProperty, new SolidColorBrush(Colors.Gray));
        template.VisualTree.SetValue(Shape.StrokeThicknessProperty, 1.0);
        template.VisualTree.SetValue(Shape.StrokeDashArrayProperty, DashStyles.Dash.Dashes);
        template.VisualTree.SetValue(Polyline.PointsProperty, new PointCollection(points));

        return template; // Return the template
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
