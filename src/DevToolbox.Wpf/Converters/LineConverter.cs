using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// Converts a <see cref="Point"/> into a <see cref="ControlTemplate"/> that displays dashed lines
/// representing horizontal and vertical axes through the given point.
/// Implements <see cref="MarkupExtension"/> for XAML support and <see cref="IValueConverter"/> for value conversion.
/// </summary>
public sealed class LineConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Provides the current instance of <see cref="LineConverter"/> for use in XAML binding.
    /// </summary>
    /// <param name="serviceProvider">The service provider that can supply services for markup extensions.</param>
    /// <returns>The current instance of <see cref="LineConverter"/>.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    #region IValueConverter Members

    /// <summary>
    /// Converts a <see cref="Point"/> to a <see cref="ControlTemplate"/> containing dashed lines.
    /// The point represents the intersection of the horizontal and vertical dashed lines.
    /// </summary>
    /// <param name="value">The <see cref="Point"/> used to define the line intersection.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="culture">The culture information (unused).</param>
    /// <returns>A <see cref="ControlTemplate"/> with dashed lines drawn through the specified point.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var p = (Point)value; // The input point
        var group = new GeometryGroup();

        // Create vertical and horizontal lines
        group.Children.Add(new LineGeometry(new Point(p.X, 0), new Point(p.X, 10000))); // Vertical line
        group.Children.Add(new LineGeometry(new Point(0, p.Y), new Point(10000, p.Y))); // Horizontal line

        // Create a ControlTemplate for displaying the lines
        var template = new ControlTemplate(typeof(Control));
        template.VisualTree = new FrameworkElementFactory(typeof(Path));
        template.VisualTree.SetValue(Shape.StrokeProperty, new SolidColorBrush(Colors.Gray)); // Set stroke color
        template.VisualTree.SetValue(Shape.StrokeDashArrayProperty, DashStyles.Dash.Dashes); // Set dashed line style
        template.VisualTree.SetValue(Shape.StrokeThicknessProperty, 1.0); // Set stroke thickness
        template.VisualTree.SetValue(Path.DataProperty, group); // Assign the geometry group

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
