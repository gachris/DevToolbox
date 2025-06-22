using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace DevToolbox.Wpf.Demo.Helpers;

public class BindableSelectedItemBehavior : Behavior<TreeView>
{
    #region SelectedItem Property

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(BindableSelectedItemBehavior),
            new UIPropertyMetadata(null, OnSelectedItemChanged));

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BindableSelectedItemBehavior behavior &&
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

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
    }

    private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedItem = e.NewValue;
    }

    // Recursively find the TreeViewItem for the given data item
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
