using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a customizable tree view item that supports drag-and-drop operations 
/// and provides functionality for editing the item.
/// </summary>
public partial class TreeViewItemEdit : TreeViewItem
{
    #region Fields/Consts

    /// <summary>
    /// Identifies the <see cref="IsItemUpdateRequired"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsItemUpdateRequiredProperty =
        DependencyProperty.Register("IsItemUpdateRequired", typeof(bool), typeof(TreeViewItemEdit), new UIPropertyMetadata(false));

    /// <summary>
    /// Identifies the <see cref="IsDraggingOver"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsDraggingOverProperty =
        DependencyProperty.Register("IsDraggingOver", typeof(bool), typeof(TreeViewItemEdit), new UIPropertyMetadata(false, OnIsDraggingOverChanged));

    /// <summary>
    /// Identifies the <see cref="ExpandIfDragOver"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ExpandIfDragOverProperty =
        DependencyProperty.Register("ExpandIfDragOver", typeof(bool), typeof(TreeViewItemEdit), new UIPropertyMetadata(true));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether an update is required for the item.
    /// </summary>
    public bool IsItemUpdateRequired
    {
        get => (bool)GetValue(IsItemUpdateRequiredProperty);
        set => SetValue(IsItemUpdateRequiredProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the item is currently being dragged over.
    /// </summary>
    public bool IsDraggingOver
    {
        get => (bool)GetValue(IsDraggingOverProperty);
        set => SetValue(IsDraggingOverProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the item should expand if an item is dragged over it.
    /// </summary>
    public bool ExpandIfDragOver
    {
        get => (bool)GetValue(ExpandIfDragOverProperty);
        set => SetValue(ExpandIfDragOverProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes the <see cref="TreeViewItemEdit"/> class.
    /// </summary>
    static TreeViewItemEdit()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeViewItemEdit), new FrameworkPropertyMetadata(typeof(TreeViewItemEdit)));

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeViewItemEdit"/> class.
    /// </summary>
    public TreeViewItemEdit()
    {
        // Add an event handler for when the item is selected.
        AddHandler(SelectedEvent, new RoutedEventHandler((obj, args) =>
        {
            if (args.OriginalSource is TreeViewItemEdit treeViewItem)
            {
                treeViewItem.BringIntoView();
            }
        }));
    }

    #region Methods Override

    /// <summary>
    /// Gets the container for the item override.
    /// </summary>
    /// <returns>A new instance of <see cref="TreeViewItemEdit"/>.</returns>
    protected override DependencyObject GetContainerForItemOverride() => new TreeViewItemEdit();

    #endregion

    #region Methods

    /// <summary>
    /// Callback for when the <see cref="IsDraggingOver"/> property changes.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    /// <param name="e">The event arguments.</param>
    private static void OnIsDraggingOverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var treeViewItem = (TreeViewItemEdit)d;
        treeViewItem.OnIsDraggingOverChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    /// <summary>
    /// Called when the dragging state changes.
    /// Expands the tree view item if dragging is over it and <see cref="ExpandIfDragOver"/> is true.
    /// </summary>
    /// <param name="oldValue">The old value of the <see cref="IsDraggingOver"/> property.</param>
    /// <param name="newValue">The new value of the <see cref="IsDraggingOver"/> property.</param>
    private void OnIsDraggingOverChanged(bool oldValue, bool newValue)
    {
        if (IsDraggingOver && ExpandIfDragOver)
        {
            var dispatcherTimer = new DispatcherTimer();
            void onTick(object? sender, EventArgs e)
            {
                if (IsDraggingOver)
                    SetValue(TreeViewItem.IsExpandedProperty, true);
                dispatcherTimer.Tick -= onTick;
                dispatcherTimer.Stop();
            }

            dispatcherTimer.Tick += onTick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
    }

    #endregion
}
