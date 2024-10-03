using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// A converter that calculates the left margin for TreeViewItems based on their depth in the tree.
/// </summary>
public class TreeViewItemLeftMarginConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the length of the margin per depth level.
    /// </summary>
    public double Length { get; set; }

    /// <summary>
    /// Converts the TreeViewItem to a Thickness based on its depth.
    /// </summary>
    /// <param name="value">The TreeViewItem for which the margin is calculated.</param>
    /// <param name="targetType">The target type of the conversion.</param>
    /// <param name="parameter">An optional parameter for the conversion.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>The calculated Thickness for the left margin.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TreeViewItem item)
            return new Thickness(0);

        return new Thickness(Length * GetDepth(item), 0, 0, 0);
    }

    /// <summary>
    /// Not implemented, as this converter does not support two-way binding.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target type for conversion.</param>
    /// <param name="parameter">An optional parameter for the conversion.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>Throws NotImplementedException.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    /// <summary>
    /// Gets the depth of the TreeViewItem within the TreeView.
    /// </summary>
    /// <param name="item">The TreeViewItem for which to get the depth.</param>
    /// <returns>The depth of the TreeViewItem.</returns>
    private static int GetDepth(TreeViewItem item)
    {
        TreeViewItem? parent;
        while ((parent = GetParent(item)) != null)
        {
            return GetDepth(parent) + 1;
        }
        return 0;
    }

    /// <summary>
    /// Gets the parent of the specified TreeViewItem in the visual tree.
    /// </summary>
    /// <param name="item">The TreeViewItem for which to get the parent.</param>
    /// <returns>The parent TreeViewItem or null if no parent is found.</returns>
    private static TreeViewItem? GetParent(TreeViewItem item)
    {
        var parent = item != null ? VisualTreeHelper.GetParent(item) : null;
        while (parent is not null and not (TreeViewItem or TreeView))
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent as TreeViewItem;
    }
}
