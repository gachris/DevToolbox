using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Extensions;

internal static class ItemsControlExtensions
{
    public static DependencyObject ContainerFromItem(this ItemsControl itemsControl, object value)
    {
        if (itemsControl.IsItemItsOwnContainer(value))
            return (DependencyObject)value;
        else
        {
            itemsControl.UpdateLayout();
            return itemsControl.ItemContainerGenerator.ContainerFromItem(value);
        }
    }

    public static object ItemFromContainer(this ItemsControl itemsControl, object value)
    {
        if (!itemsControl.IsItemItsOwnContainer(value))
            return value;
        else
        {
            itemsControl.UpdateLayout();
            var item = itemsControl.ItemContainerGenerator.ItemFromContainer((DependencyObject)value);
            if (item == DependencyProperty.UnsetValue)
            {
                var index = itemsControl.ItemContainerGenerator.IndexFromContainer((DependencyObject)value);
                if (index != -1)
                    item = itemsControl.Items[index];
            }
            return item;
        }
    }
}