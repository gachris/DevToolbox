using System;
using System.Windows;
using System.Windows.Media;

namespace DevToolbox.Wpf.Extensions;

internal static class DependencyObjectExtensions
{
    public static T? FindVisualAncestor<T>(this DependencyObject element, Func<T, bool>? filter = null) where T : DependencyObject
    {
        filter ??= (depObj => true);
        while (element is not null and Visual)
        {
            if (element is T o && filter(o))
                return o;

            element = VisualTreeHelper.GetParent(element);
        }
        return default;
    }
}