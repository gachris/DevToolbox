using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace DevToolbox.Wpf.Behaviors;

/// <summary>
/// A behavior that enables binding the SelectedItem property of a TreeView.
/// </summary>
public class TreeViewBehavior : Behavior<TreeView>
{
    #region SelectedItem Property

    /// <summary>
    /// Gets or sets the selected item in the TreeView.
    /// </summary>
    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Identifies the SelectedItem dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(TreeViewBehavior),
            new UIPropertyMetadata(null, OnSelectedItemChanged));

    /// <summary>
    /// Handles changes to the SelectedItem property.
    /// Selects and focuses the corresponding TreeViewItem if found.
    /// </summary>
    /// <param name="d">The dependency object on which the property changed.</param>
    /// <param name="e">Event arguments for the property change.</param>
    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TreeViewBehavior behavior &&
            behavior.AssociatedObject is TreeView treeView &&
            e.NewValue is object dataItem)
        {
            treeView.Dispatcher.BeginInvoke(() =>
            {
                var container = GetTreeViewItem(treeView, dataItem);
                if (container != null)
                {
                    container.IsSelected = true;
                    container.Focus(); // Optional: bring into view
                }
            });
        }
    }

    #endregion

    /// <summary>
    /// Called when the behavior is attached to a TreeView.
    /// Subscribes to the SelectedItemChanged event.
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
    }

    /// <summary>
    /// Called when the behavior is detached from a TreeView.
    /// Unsubscribes from the SelectedItemChanged event.
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
    }

    /// <summary>
    /// Handles the TreeView's SelectedItemChanged event and updates the SelectedItem property.
    /// </summary>
    private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedItem = e.NewValue;
    }

    /// <summary>
    /// Recursively searches for a TreeViewItem corresponding to the given data item.
    /// </summary>
    /// <param name="container">The parent ItemsControl to search within.</param>
    /// <param name="item">The data item to find.</param>
    /// <returns>The TreeViewItem corresponding to the item, or null if not found.</returns>
    private static TreeViewItem? GetTreeViewItem(ItemsControl container, object item)
    {
        if (container == null)
            return null;

        foreach (var childItem in container.Items)
        {
            if (container.ItemContainerGenerator.ContainerFromItem(childItem) is TreeViewItem treeViewItem)
            {
                if (childItem == item)
                    return treeViewItem;

                var child = GetTreeViewItem(treeViewItem, item);
                if (child != null)
                    return child;
            }
        }

        return null;
    }
}
