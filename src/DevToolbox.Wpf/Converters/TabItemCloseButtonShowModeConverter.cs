using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// A converter that determines the visibility of the close button on a tab item.
/// </summary>
public class TabItemCloseButtonShowModeConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts the values to a visibility state for the close button.
    /// </summary>
    /// <param name="value">An array containing the close button show mode, whether the tab is selected, and whether the mouse is over the tab.</param>
    /// <param name="targetType">The target type of the conversion.</param>
    /// <param name="parameter">The parameter for conversion.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>The visibility of the close button.</returns>
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        var closeButtonShowMode = (CloseButtonShowMode)value[0];
        var isSelected = (bool)value[1];
        var isMouseOver = (bool)value[2];

        return closeButtonShowMode switch
        {
            CloseButtonShowMode.InAllTabs => Visibility.Visible,
            CloseButtonShowMode.InActiveTab when isSelected => Visibility.Visible,
            CloseButtonShowMode.InActiveAndMouseOverTabs when isSelected => Visibility.Visible,
            CloseButtonShowMode.InActiveAndMouseOverTabs when isMouseOver => Visibility.Visible,
            CloseButtonShowMode.InAllTabsAndTabControl => Visibility.Visible,
            CloseButtonShowMode.InActiveAndMouseOverTabsAndTabControl when isSelected => Visibility.Visible,
            CloseButtonShowMode.InActiveAndMouseOverTabsAndTabControl when isMouseOver => Visibility.Visible,
            CloseButtonShowMode.InTabControl => Visibility.Collapsed,
            _ => Visibility.Collapsed
        };
    }

    /// <summary>
    /// Not implemented, as this converter does not support two-way binding.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target types for conversion.</param>
    /// <param name="parameter">The parameter for conversion.</param>
    /// <param name="culture">The culture information for conversion.</param>
    /// <returns>Throws NotImplementedException.</returns>
    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
