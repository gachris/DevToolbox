using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevToolbox.Wpf.Extensions;

internal static class DependencyObjectExtensions
{
    /// <summary>
    /// Searches the subtree of an element (including that element)
    /// for an element of a particular type.
    /// </summary>
    public static T? FindElementOfType<T>(this FrameworkElement? element) where T : FrameworkElement
    {
        if (element is T correctlyTyped)
        {
            return correctlyTyped;
        }

        if (element != null)
        {
            var numChildren = VisualTreeHelper.GetChildrenCount(element);
            for (var i = 0; i < numChildren; i++)
            {
                if (VisualTreeHelper.GetChild(element, i) is FrameworkElement childElement)
                {
                    var child = FindElementOfType<T>(childElement);
                    if (child != null)
                    {
                        return child;
                    }
                }
            }

            if (element is Popup popup && popup.Child is FrameworkElement popupChild)
            {
                return FindElementOfType<T>(popupChild);
            }
        }

        return null;
    }

    /// <summary>
    ///     Returns the template element of the given name within the Control.
    /// </summary>
    public static T? FindName<T>(this Control control, string name) where T : FrameworkElement
    {
        ControlTemplate template = control.Template;
        return template != null ? template.FindName(name, control) as T : null;
    }

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

    public static T? FindVisualChild<T>(this DependencyObject parent) where T : Visual
    {
        var child = default(T);
        var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < numVisuals; i++)
        {
            var v = (Visual)VisualTreeHelper.GetChild(parent, i);
            child = v as T;
            child ??= FindVisualChild<T>(v);
            if (child != null)
            {
                break;
            }
        }
        return child;
    }

    public static T? FindVisualChildByName<T>(this DependencyObject parent, string childName) where T : DependencyObject
    {
        // Confirm parent and childName are valid.  
        if (parent == null) return null;
        T? foundChild = null;
        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child  
            if (child is not T t)
            {
                // recursively drill down the tree  
                foundChild = FindVisualChildByName<T>(child, childName);
                // If the child is found, break so we do not overwrite the found child.    
                if (foundChild != null) break;
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                // If the child's name is set for search    
                if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                {
                    // if the child's name is of the request name  
                    foundChild = t;
                    break;
                }
            }
            else
            {
                // child element found.
                foundChild = t;
                break;
            }
        }
        return foundChild;
    }

    public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject d) where T : DependencyObject
    {
        if (d != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(d, i);

                if (child != null)
                {
                    if (child is T t)
                        yield return t;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
    }

    /// <summary>
    /// Searches up the visual tree from the specified node to find an ancestor of type T.
    /// </summary>
    /// <typeparam name="T">The type of ancestor to find.</typeparam>
    /// <param name="node">The starting node.</param>
    /// <returns>The first ancestor of type T, or null if none found.</returns>
    public static T? VisualUpwardSearch<T>(this DependencyObject? node) where T : DependencyObject
    {
        while (node != null)
        {
            if (node is T found) return found;
            node = VisualTreeHelper.GetParent(node);
        }
        return null;
    }
}