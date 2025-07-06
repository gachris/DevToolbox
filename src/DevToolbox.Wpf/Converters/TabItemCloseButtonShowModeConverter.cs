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
        // assume CloseButtonShowMode.None is the default you want when it’s not provided
        CloseButtonShowMode closeButtonShowMode = default;
        bool isSelected = false;
        bool isMouseOver = false;

        // value is object[] — check length and type before assigning
        if (value.Length > 0 && value[0] is CloseButtonShowMode mode)
            closeButtonShowMode = mode;

        if (value.Length > 1 && value[1] is bool sel)
            isSelected = sel;

        if (value.Length > 2 && value[2] is bool over)
            isMouseOver = over;

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
