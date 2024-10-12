using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a customizable tab control that supports adding, removing, and swapping tabs.
/// This control allows for enhanced tab management features such as showing and hiding buttons,
/// customizing the appearance of tabs, and handling events related to tab actions.
/// </summary>
public class TabControlEdit : TabControl
{
    #region Fields/Consts

    private static readonly RoutedUICommand _addCommand = ApplicationCommands.New;
    private static readonly RoutedUICommand _closeCommand = ApplicationCommands.Close;
    private static readonly RoutedUICommand _swapCommand = new(nameof(SwapCommand), nameof(SwapCommand), typeof(TabControlEdit));

    /// <summary>
    /// Occurs when the tab control is closing, allowing for cancellation of the operation.
    /// </summary>
    public event CancelEventHandler? Closing;

    /// <summary>
    /// Occurs after the tab control has been closed.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// An event that is raised when a new item is created so that
    /// developers can initialize the item with custom default values.
    /// </summary>
    public event InitializingNewItemEventHandler? InitializingNewItem;

    /// <summary>
    /// An event that is raised before a new item is created so that
    /// developers can participate in the construction of the new item.
    /// </summary>
    public event EventHandler<AddingNewItemEventArgs>? AddingNewItem;

    /// <summary>
    /// Dependency property for the TabPanel view mode.
    /// </summary>
    public static readonly DependencyProperty TabPanelViewModeProperty =
        DependencyProperty.Register(nameof(TabPanelViewMode), typeof(TabPanelViewMode), typeof(TabControlEdit), new PropertyMetadata());

    /// <summary>
    /// Dependency property for the style of the close tab control button.
    /// </summary>
    public static readonly DependencyProperty CloseTabControlButtonStyleProperty =
        DependencyProperty.Register(nameof(CloseTabControlButtonStyle), typeof(Style), typeof(TabControlEdit));

    /// <summary>
    /// Dependency property for the style of the swap tabs button.
    /// </summary>
    public static readonly DependencyProperty SwapTabsButtonStyleProperty =
        DependencyProperty.Register(nameof(SwapTabsButtonStyle), typeof(Style), typeof(TabControlEdit));

    /// <summary>
    /// Dependency property for the style of the add tab button.
    /// </summary>
    public static readonly DependencyProperty AddTabButtonStyleProperty =
        DependencyProperty.Register(nameof(AddTabButtonStyle), typeof(Style), typeof(TabControlEdit));

    /// <summary>
    /// Dependency property for the mode in which close buttons are shown.
    /// </summary>
    public static readonly DependencyProperty CloseButtonShowModeProperty =
        DependencyProperty.Register(nameof(CloseButtonShowMode), typeof(CloseButtonShowMode), typeof(TabControlEdit), new PropertyMetadata(CloseButtonShowMode.InAllTabs));

    /// <summary>
    /// Dependency property indicating whether the add tab button should be displayed.
    /// </summary>
    public static readonly DependencyProperty ShowAddTabButtonProperty =
        DependencyProperty.Register(nameof(ShowAddTabButton), typeof(bool), typeof(TabControlEdit), new PropertyMetadata(true));

    /// <summary>
    /// Dependency property for the visibility mode of the swap tabs button.
    /// </summary>
    public static readonly DependencyProperty SwapTabsButtonShowModeProperty =
        DependencyProperty.Register(nameof(SwapTabsButtonShowMode), typeof(SwapTabsButtonShowMode), typeof(TabControlEdit), new PropertyMetadata(SwapTabsButtonShowMode.Visible));

    /// <summary>
    /// Dependency property for the horizontal alignment of the tab panel.
    /// </summary>
    public static readonly DependencyProperty TabPanelHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(TabPanelHorizontalAlignment), typeof(HorizontalAlignment), typeof(TabControlEdit));

    /// <summary>
    /// Dependency property for the vertical alignment of the tab panel.
    /// </summary>
    public static readonly DependencyProperty TabPanelVerticalAlignmentProperty =
        DependencyProperty.Register(nameof(TabPanelVerticalAlignment), typeof(VerticalAlignment), typeof(TabControlEdit));

    /// <summary>
    /// Dependency property for the style of the tab left scroll button.
    /// </summary>
    public static readonly DependencyProperty TabLeftScrollButtonStyleProperty =
        DependencyProperty.Register(nameof(TabLeftScrollButtonStyle), typeof(Style), typeof(TabControlEdit));

    /// <summary>
    /// Dependency property for the style of the tab bottom left scroll button.
    /// </summary>
    public static readonly DependencyProperty TabBottomScrollButtonStyleProperty =
        DependencyProperty.Register(nameof(TabBottomScrollButtonStyle), typeof(Style), typeof(TabControlEdit));

    /// <summary>
    /// Dependency property for the style of the tab top scroll button.
    /// </summary>
    public static readonly DependencyProperty TabTopScrollButtonStyleProperty =
        DependencyProperty.Register(nameof(TabTopScrollButtonStyle), typeof(Style), typeof(TabControlEdit));

    /// <summary>
    /// Dependency property for the style of the tab right scroll button.
    /// </summary>
    public static readonly DependencyProperty TabRightScrollButtonStyleProperty =
        DependencyProperty.Register(nameof(TabRightScrollButtonStyle), typeof(Style), typeof(TabControlEdit));

    #endregion

    #region Properties

    /// <summary>
    /// Gets the command for adding a new tab.
    /// </summary>
    public static RoutedUICommand AddCommand => _addCommand;

    /// <summary>
    /// Gets the command for closing a tab.
    /// </summary>
    public static RoutedUICommand CloseCommand => _closeCommand;

    /// <summary>
    /// Gets the command for swapping tabs.
    /// </summary>
    public static RoutedCommand SwapCommand => _swapCommand;

    /// <summary>
    /// Gets the editable items in the tab control.
    /// </summary>
    private IEditableCollectionView EditableItems => Items;

    /// <summary>
    /// Gets or sets the view mode of the tab panel.
    /// </summary>
    public TabPanelViewMode TabPanelViewMode
    {
        get => (TabPanelViewMode)GetValue(TabPanelViewModeProperty);
        set => SetValue(TabPanelViewModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the add tab button is shown.
    /// </summary>
    public bool ShowAddTabButton
    {
        get => (bool)GetValue(ShowAddTabButtonProperty);
        set => SetValue(ShowAddTabButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets the mode for showing the close button.
    /// </summary>
    public CloseButtonShowMode CloseButtonShowMode
    {
        get => (CloseButtonShowMode)GetValue(CloseButtonShowModeProperty);
        set => SetValue(CloseButtonShowModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the visibility mode of the swap tabs button.
    /// </summary>
    public SwapTabsButtonShowMode SwapTabsButtonShowMode
    {
        get => (SwapTabsButtonShowMode)GetValue(SwapTabsButtonShowModeProperty);
        set => SetValue(SwapTabsButtonShowModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the tab panel.
    /// </summary>
    public HorizontalAlignment TabPanelHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(TabPanelHorizontalAlignmentProperty);
        set => SetValue(TabPanelHorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the tab panel.
    /// </summary>
    public VerticalAlignment TabPanelVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(TabPanelVerticalAlignmentProperty);
        set => SetValue(TabPanelVerticalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the add tab button.
    /// </summary>
    public Style AddTabButtonStyle
    {
        get => (Style)GetValue(AddTabButtonStyleProperty);
        set => SetValue(AddTabButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the swap tabs button.
    /// </summary>
    public Style SwapTabsButtonStyle
    {
        get => (Style)GetValue(SwapTabsButtonStyleProperty);
        set => SetValue(SwapTabsButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the close tab control button.
    /// </summary>
    public Style CloseTabControlButtonStyle
    {
        get => (Style)GetValue(CloseTabControlButtonStyleProperty);
        set => SetValue(CloseTabControlButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the tab left scroll button.
    /// </summary>
    [Description("Gets or sets the tab left button style")]
    [Category("ScrollButton")]
    public Style TabLeftScrollButtonStyle
    {
        get => (Style)GetValue(TabLeftScrollButtonStyleProperty);
        set => SetValue(TabLeftScrollButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the tab right scroll button.
    /// </summary>
    [Description("Gets or sets the tab right button style")]
    [Category("ScrollButton")]
    public Style TabRightScrollButtonStyle
    {
        get => (Style)GetValue(TabRightScrollButtonStyleProperty);
        set => SetValue(TabRightScrollButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the tab top scroll button.
    /// </summary>
    [Description("Gets or sets the tab top button style")]
    [Category("ScrollButton")]
    public Style TabTopScrollButtonStyle
    {
        get => (Style)GetValue(TabTopScrollButtonStyleProperty);
        set => SetValue(TabTopScrollButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the tab bottom scroll button.
    /// </summary>
    [Description("Gets or sets the tab bottom button style")]
    [Category("ScrollButton")]
    public Style TabBottomScrollButtonStyle
    {
        get => (Style)GetValue(TabBottomScrollButtonStyleProperty);
        set => SetValue(TabBottomScrollButtonStyleProperty, value);
    }

    #endregion

    static TabControlEdit()
    {
        Type typeFromHandle = typeof(TabControlEdit);

        DefaultStyleKeyProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(typeFromHandle));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
            new(AddCommand, (sender, e) => (sender as TabControlEdit)?.AddExecuted(e), (sender, e) => (sender as TabControlEdit)?.AddCanExecute(e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
            new(SwapCommand, (sender, e) => (sender as TabControlEdit)?.SwapExecuted(e), (sender, e) => (sender as TabControlEdit)?.SwapCanExecute(e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
            new(CloseCommand, (sender, e) => (sender as TabControlEdit)?.CloseExecuted(e), (sender, e) => (sender as TabControlEdit)?.CloseCanExecute(e)));
    }

    #region Methods Overrides

    /// <summary>
    /// Gets the container for each item in the tab control.
    /// </summary>
    /// <returns>A new instance of the <see cref="TabItemEdit"/> class.</returns>
    protected override DependencyObject GetContainerForItemOverride() => new TabItemEdit();

    #endregion

    #region Methods

    /// <summary>
    /// Handles the execution of the Add command.
    /// Creates a new item and selects it.
    /// </summary>
    /// <param name="e">Event arguments containing data related to the command execution.</param>
    protected virtual void AddExecuted(ExecutedRoutedEventArgs e)
    {
        var item = AddNewItem();
        SelectedIndex = Items.IndexOf(item);
    }

    /// <summary>
    /// Determines whether the Add command can execute based on the current state.
    /// </summary>
    /// <param name="e">Event arguments that indicate whether the command can execute.</param>
    protected virtual void AddCanExecute(CanExecuteRoutedEventArgs e) => e.CanExecute = EditableItems.CanAddNew || !((IList)Items).IsReadOnly;

    /// <summary>
    /// Handles the execution of the Swap command.
    /// </summary>
    /// <param name="e">Event arguments containing data related to the command execution.</param>
    protected virtual void SwapExecuted(ExecutedRoutedEventArgs e) => SwapTabs();

    /// <summary>
    /// Determines whether the Swap command can execute based on the current state.
    /// </summary>
    /// <param name="e">Event arguments that indicate whether the command can execute.</param>
    protected virtual void SwapCanExecute(CanExecuteRoutedEventArgs e) => e.CanExecute = Items.Count >= 2;

    /// <summary>
    /// Handles the execution of the Close command.
    /// Closes the currently selected item.
    /// </summary>
    /// <param name="e">Event arguments containing data related to the command execution.</param>
    protected virtual void CloseExecuted(ExecutedRoutedEventArgs e) => Close(SelectedItem);

    /// <summary>
    /// Determines whether the Close command can execute based on the current state.
    /// </summary>
    /// <param name="e">Event arguments that indicate whether the command can execute.</param>
    protected virtual void CloseCanExecute(CanExecuteRoutedEventArgs e) => e.CanExecute = EditableItems.CanRemove || !((IList)Items).IsReadOnly && SelectedItem != null;

    /// <summary>
    /// Adds a new item to the tab control.
    /// </summary>
    /// <returns>The newly created item.</returns>
    public object Add() => AddNewItem();

    /// <summary>
    /// Adds a new item to the tab control with the specified object.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    /// <returns>The newly created item.</returns>
    public object Add(object item) => AddNewItem(item);

    /// <summary>
    /// Adds a new item to the tab control, optionally using the provided object.
    /// </summary>
    /// <param name="item">The object to use for creating the new item, or null for a default item.</param>
    /// <returns>The newly created item.</returns>
    private object AddNewItem(object? item = null)
    {
        var addNewItem = (IEditableCollectionViewAddNewItem)Items;
        var isReadOnly = ((IList)Items).IsReadOnly;

        if (addNewItem.CanAddNewItem || !isReadOnly)
        {
            var e = new AddingNewItemEventArgs { NewItem = item };
            OnAddingNewItem(e);
            item = e.NewItem;
        }

        if (!isReadOnly)
        {
            if (item is null)
            {
                var generator = ItemContainerGenerator as IItemContainerGenerator;

                item = GetContainerForItemOverride();
                generator.PrepareItemContainer((DependencyObject)item);
            }
            else Items.Add(item);
        }
        else
        {
            item = item is not null ? addNewItem.AddNewItem(item) : EditableItems.AddNew();

            EditableItems.CommitNew();
        }

        OnInitializingNewItem(new InitializingNewItemEventArgs(item));

        CommandManager.InvalidateRequerySuggested();

        return item;
    }

    /// <summary>
    /// Removes the specified item from the tab control.
    /// </summary>
    /// <param name="item">The item to be removed.</param>
    public void Remove(object item)
    {
        var isReadOnly = ((IList)Items).IsReadOnly;

        if (!isReadOnly)
            Items.Remove(item);
        else EditableItems.Remove(item);
    }

    /// <summary>
    /// Closes the specified item in the tab control, invoking any necessary events.
    /// </summary>
    /// <param name="item">The item to be closed.</param>
    public void Close(object item)
    {
        var e = new CancelEventArgs();

        Closing?.Invoke(item, e);
        OnClosing(item, e);

        if (!e.Cancel)
            Closed?.Invoke(item, new EventArgs());
    }

    /// <summary>
    /// Handles the closing logic for the specified item.
    /// </summary>
    /// <param name="item">The item to close.</param>
    /// <param name="e">Event arguments indicating whether the closing should be canceled.</param>
    private void OnClosing(object item, CancelEventArgs e)
    {
        if (!e.Cancel)
        {
            var isReadOnly = ((IList)Items).IsReadOnly;

            if (!isReadOnly)
                Items.Remove(item);
            else EditableItems.Remove(item);
        }
    }

    /// <summary>
    /// Swaps the currently selected tabs in the tab control.
    /// This method should be implemented to define the swapping behavior.
    /// </summary>
    private void SwapTabs()
    {
        // Implement the swap logic here
    }

    /// <summary>
    /// A method that is called before a new item is created so that
    /// overrides can participate in the construction of the new item.
    /// </summary>
    /// <remarks>
    /// The default implementation raises the AddingNewItem event.
    /// </remarks>
    /// <param name="e">Event arguments that provide access to the new item.</param>
    protected virtual void OnAddingNewItem(AddingNewItemEventArgs e) => AddingNewItem?.Invoke(this, e);

    /// <summary>
    /// A method that is called when a new item is created so that
    /// overrides can initialize the item with custom default values.
    /// </summary>
    /// <remarks>
    /// The default implementation raises the InitializingNewItem event.
    /// </remarks>
    /// <param name="e">Event arguments that provide access to the new item.</param>
    protected virtual void OnInitializingNewItem(InitializingNewItemEventArgs e) => InitializingNewItem?.Invoke(this, e);


    #endregion
}