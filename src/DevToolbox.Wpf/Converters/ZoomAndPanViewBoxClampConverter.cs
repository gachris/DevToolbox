using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// A converter used for clamping values related to zooming and panning functionality in the ZoomAndPanControl.
/// This converter is utilized to limit the size of a UI element based on zoom, offset, and available space.
/// </summary>
public class ZoomAndPanViewBoxClampConverter : MarkupExtension, IMultiValueConverter
{
    /// <summary>
    /// Provides an instance of the converter to be used by XAML.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The instance of the ZoomAndPanViewBoxClampConverter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    /// <summary>
    /// Converts the provided values into a clamped size value based on the zoom, offset, and extent of the ZoomAndPanControl.
    /// </summary>
    /// <param name="values">An array of objects containing the size, offset, zoom, and the ZoomAndPanControl.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">An optional parameter to indicate whether width or height is being calculated.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>A clamped size value that ensures the content does not exceed the visible boundaries.</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // NOTE: ExtentWidth and ExtentHeight do not update dynamically, so they are not passed as values
        var zoomAndPanControl = (ZoomAndPanControl)values[3];
        if (values[0] == null || zoomAndPanControl == null) return DependencyProperty.UnsetValue;
        var size = (double)values[0];
        var offset = (double)values[1];
        var zoom = (double)values[2];
        return Math.Max((parameter?.ToString()?.ToLower() == "width")
             ? Math.Min(zoomAndPanControl.ExtentWidth / zoom - offset, size)
             : Math.Min(zoomAndPanControl.ExtentHeight / zoom - offset, size), 0);
    }

    /// <summary>
    /// ConvertBack is not implemented because the conversion is one-way.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetTypes">The array of target types for the binding.</param>
    /// <param name="parameter">An optional parameter for the conversion.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>Throws a NotImplementedException as the conversion is not reversible.</returns>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}