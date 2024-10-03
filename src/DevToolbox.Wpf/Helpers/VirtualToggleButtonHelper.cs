﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DevToolbox.Wpf.Helpers;

/// <summary>
/// Provides attached properties and methods for enabling virtual toggle button behavior 
/// on WPF controls, especially within tree views, allowing for checked state management 
/// and event handling (Checked, Unchecked, Indeterminate).
/// </summary>
public static class VirtualToggleButtonHelper
{
    #region attached properties

    #region IsChecked

    /// <summary>
    /// IsChecked Attached Dependency Property
    /// </summary>
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.RegisterAttached("IsChecked", typeof(bool?), typeof(VirtualToggleButtonHelper),
            new FrameworkPropertyMetadata((bool?)false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                new PropertyChangedCallback(OnIsCheckedChanged)));

    /// <summary>
    /// Gets the IsChecked property.  This dependency property 
    /// indicates whether the toggle button is checked.
    /// </summary>
    public static bool? GetIsChecked(DependencyObject d)
    {
        return (bool?)d.GetValue(IsCheckedProperty);
    }

    /// <summary>
    /// Sets the IsChecked property.  This dependency property 
    /// indicates whether the toggle button is checked.
    /// </summary>
    public static void SetIsChecked(DependencyObject d, bool? value)
    {
        d.SetValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// Handles changes to the IsChecked property.
    /// </summary>
    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TreeViewItem pseudobutton)
        {
            var newValue = (bool?)e.NewValue;
            if (newValue == true)
            {
                RaiseCheckedEvent(pseudobutton);
            }
            else if (newValue == false)
            {
                RaiseUncheckedEvent(pseudobutton);
            }
            else
            {
                RaiseIndeterminateEvent(pseudobutton);
            }

            SetIsChecked(pseudobutton, (bool?)e.NewValue, true, true);
        }
    }

    private static void SetIsChecked(TreeViewItem treeViewItem, bool? value, bool updateChildren, bool updateParent)
    {
        SetIsChecked(treeViewItem, value);

        if (updateChildren && value.HasValue)
        {
            var items = treeViewItem.Items ?? treeViewItem.ItemsSource;
            if (items == null)
                return;

            foreach (var item in items)
            {
                var child = item is TreeViewItem edit ? edit : treeViewItem.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (child != null)
                {
                    SetIsChecked(child, value, true, false);
                }
            }
        }

        var parent = treeViewItem.Parent ?? treeViewItem.FindVisualAncestor<TreeViewItem>();
        if (updateParent && parent is TreeViewItem treeViewItem1)
            VerifyCheckState(treeViewItem1);
    }

    private static void VerifyCheckState(TreeViewItem treeViewItem)
    {
        var items = treeViewItem.Items.Cast<object>().ToList() ?? treeViewItem.ItemsSource.Cast<object>().ToList();
        if (items == null)
            return;

        var state = default(bool?);
        for (var i = 0; i < items.Count; ++i)
        {
            var item = items[i];
            var child = item is TreeViewItem edit ? edit : treeViewItem.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
            if (child != null)
            {
                if (i == 0)
                    state = GetIsChecked(child);
                else if (state != GetIsChecked(child))
                {
                    state = null;
                    break;
                }
            }
        }

        SetIsChecked(treeViewItem, state, false, true);
    }

    private static T? FindVisualAncestor<T>(this DependencyObject element, Func<T, bool>? filter = null) where T : DependencyObject
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

    #endregion

    #region IsThreeState

    /// <summary>
    /// IsThreeState Attached Dependency Property
    /// </summary>
    public static readonly DependencyProperty IsThreeStateProperty =
        DependencyProperty.RegisterAttached("IsThreeState", typeof(bool), typeof(VirtualToggleButtonHelper),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Gets the IsThreeState property.  This dependency property 
    /// indicates whether the control supports two or three states.  
    /// IsChecked can be set to null as a third state when IsThreeState is true.
    /// </summary>
    public static bool GetIsThreeState(DependencyObject d)
    {
        return (bool)d.GetValue(IsThreeStateProperty);
    }

    /// <summary>
    /// Sets the IsThreeState property.  This dependency property 
    /// indicates whether the control supports two or three states. 
    /// IsChecked can be set to null as a third state when IsThreeState is true.
    /// </summary>
    public static void SetIsThreeState(DependencyObject d, bool value)
    {
        d.SetValue(IsThreeStateProperty, value);
    }

    #endregion

    #region IsVirtualToggleButton

    /// <summary>
    /// IsVirtualToggleButton Attached Dependency Property
    /// </summary>
    public static readonly DependencyProperty IsVirtualToggleButtonProperty =
        DependencyProperty.RegisterAttached("IsVirtualToggleButton", typeof(bool), typeof(VirtualToggleButtonHelper),
            new FrameworkPropertyMetadata((bool)false,
                new PropertyChangedCallback(OnIsVirtualToggleButtonChanged)));

    /// <summary>
    /// Gets the IsVirtualToggleButton property.  This dependency property 
    /// indicates whether the object to which the property is attached is treated as a VirtualToggleButton.  
    /// If true, the object will respond to keyboard and mouse input the same way a ToggleButton would.
    /// </summary>
    public static bool GetIsVirtualToggleButton(DependencyObject d)
    {
        return (bool)d.GetValue(IsVirtualToggleButtonProperty);
    }

    /// <summary>
    /// Sets the IsVirtualToggleButton property.  This dependency property 
    /// indicates whether the object to which the property is attached is treated as a VirtualToggleButton.  
    /// If true, the object will respond to keyboard and mouse input the same way a ToggleButton would.
    /// </summary>
    public static void SetIsVirtualToggleButton(DependencyObject d, bool value)
    {
        d.SetValue(IsVirtualToggleButtonProperty, value);
    }

    /// <summary>
    /// Handles changes to the IsVirtualToggleButton property.
    /// </summary>
    private static void OnIsVirtualToggleButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IInputElement element)
        {
            if ((bool)e.NewValue)
            {
                element.MouseLeftButtonDown += OnMouseLeftButtonDown;
                element.KeyDown += OnKeyDown;
            }
            else
            {
                element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                element.KeyDown -= OnKeyDown;
            }
        }
    }

    #endregion

    #endregion

    #region routed events

    #region Checked

    /// <summary>
    /// A static helper method to raise the Checked event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs? RaiseCheckedEvent(UIElement target)
    {
        if (target == null) return null;

        var args = new RoutedEventArgs
        {
            RoutedEvent = ToggleButton.CheckedEvent
        };
        RaiseEvent(target, args);
        return args;
    }

    #endregion

    #region Unchecked

    /// <summary>
    /// A static helper method to raise the Unchecked event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs? RaiseUncheckedEvent(UIElement target)
    {
        if (target == null) return null;

        var args = new RoutedEventArgs
        {
            RoutedEvent = ToggleButton.UncheckedEvent
        };
        RaiseEvent(target, args);
        return args;
    }

    #endregion

    #region Indeterminate

    /// <summary>
    /// A static helper method to raise the Indeterminate event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs? RaiseIndeterminateEvent(UIElement target)
    {
        if (target == null) return null;

        var args = new RoutedEventArgs
        {
            RoutedEvent = ToggleButton.IndeterminateEvent
        };
        RaiseEvent(target, args);
        return args;
    }

    #endregion

    #endregion

    #region private methods

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        UpdateIsChecked((DependencyObject)sender);
    }

    private static void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.OriginalSource == sender)
        {
            if (e.Key == Key.Space)
            {
                // ignore alt+space which invokes the system menu
                if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) return;

                UpdateIsChecked((DependencyObject)sender);
                e.Handled = true;

            }
            else if (e.Key == Key.Enter && (bool)((DependencyObject)sender).GetValue(KeyboardNavigation.AcceptsReturnProperty))
            {
                UpdateIsChecked((DependencyObject)sender);
                e.Handled = true;
            }
        }
    }

    private static void UpdateIsChecked(DependencyObject d)
    {
        var isChecked = GetIsChecked(d);
        if (isChecked == true)
        {
            SetIsChecked(d, GetIsThreeState(d) ? null : false);
        }
        else
        {
            SetIsChecked(d, isChecked.HasValue);
        }
    }

    private static void RaiseEvent(DependencyObject target, RoutedEventArgs args)
    {
        if (target is UIElement element)
        {
            element.RaiseEvent(args);
        }
        else if (target is ContentElement contentElement)
        {
            contentElement.RaiseEvent(args);
        }
    }

    #endregion
}