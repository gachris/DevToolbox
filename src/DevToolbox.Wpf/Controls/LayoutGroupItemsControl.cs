using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml;
using DevToolbox.Wpf.Serialization;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a list of documents within a <see cref="LayoutManager"/>.
/// Supports adding, removing, and serializing layout of document items.
/// </summary>
public class LayoutGroupItemsControl : ItemsControl, ILayoutSerializable
{
    #region Fields/Consts

    private LayoutManager? _dockManager;
    private LayoutGroupPanel? _layoutGroupPanel;

    /// <summary>
    /// Occurs before a new item is created, allowing handlers to provide or modify the new item instance.
    /// </summary>
    public event InitializingNewItemEventHandler? InitializingNewItem;

    /// <summary>
    /// Occurs when a new item is being added, enabling custom construction logic.
    /// </summary>
    public event EventHandler<AddingNewItemEventArgs>? AddingNewItem;

    #endregion

    #region Properties

    internal LayoutGroupPanel? LayoutGroupPanel => _layoutGroupPanel;

    /// <summary>
    /// Gets the editable view of the items collection.
    /// </summary>
    private IEditableCollectionView EditableItems => Items;

    /// <inheritdoc/>
    public Rect SurfaceRectangle => new(PointToScreen(new Point(0, 0)), new Size(ActualWidth, ActualHeight));

    #endregion

    static LayoutGroupItemsControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutGroupItemsControl), new FrameworkPropertyMetadata(typeof(LayoutGroupItemsControl)));
    }

    #region Methods Override

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _layoutGroupPanel = Template.FindName("PART_LayoutGroupPanel", this) as LayoutGroupPanel;
    }

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is LayoutItemsControl documentControl)
        {
            if (documentControl.State == LayoutItemState.Document)
                _dockManager?.DragServices.Register(documentControl);

            documentControl.StateChanged += StateChanged;
            documentControl.DockChanged += OnDockChanged;

            _layoutGroupPanel?.Add(documentControl);
        }
    }

    /// <inheritdoc />
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is LayoutItemsControl documentControl)
        {
            _dockManager?.DragServices.Unregister(documentControl);

            documentControl.DockChanged -= OnDockChanged;
            documentControl.StateChanged -= StateChanged;

            _layoutGroupPanel?.Remove(documentControl);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new LayoutItemsControl();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Associates this list with the specified <see cref="LayoutManager"/>, enabling drag services.
    /// </summary>
    /// <param name="dockManager">The dock manager to attach.</param>
    internal void AttachDockManager(LayoutManager dockManager)
    {
        _dockManager = dockManager;
    }

    /// <summary>
    /// Handles a change in docking, triggering layout rearrangement.
    /// </summary>
    private void OnDockChanged(object? sender, LayoutDockChangedEventArgs e)
    {
        _layoutGroupPanel?.ArrangeLayout();
    }

    /// <summary>
    /// Handles state changes on a document control, registering or unregistering drag services.
    /// </summary>
    private void StateChanged(object? sender, LayoutItemStateChangedEventArgs e)
    {
        if (sender is not IDropSurface dropSurface)
        {
            return;
        }

        if (e.NewValue is LayoutItemState.Document or LayoutItemState.Docking)
            _dockManager?.DragServices.Register(dropSurface);
        else
            _dockManager?.DragServices.Unregister(dropSurface);

        _layoutGroupPanel?.ArrangeLayout();
    }

    /// <summary>
    /// Moves a document control relative to another within the panel.
    /// </summary>
    /// <param name="control">The control to move.</param>
    /// <param name="relativeControl">The control to move relative to.</param>
    /// <param name="relativeDock">The dock position to use.</param>
    internal void MoveTo(LayoutItemsControl control, LayoutItemsControl relativeControl, Dock relativeDock)
    {
        control.State = LayoutItemState.Document;

        _layoutGroupPanel?.Remove(control);
        _layoutGroupPanel?.Add(control, relativeControl, relativeDock);
    }

    /// <summary>
    /// Adds a new document item to the list, raising appropriate events.
    /// </summary>
    /// <returns>The newly added item.</returns>
    public object Add()
    {
        return AddNewItem();
    }

    /// <summary>
    /// Adds the specified item to the list, raising appropriate events.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The added item.</returns>
    public object Add(object item)
    {
        return AddNewItem(item);
    }

    /// <summary>
    /// Core logic for adding a new item, handling editable collection view and event invocation.
    /// </summary>
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
    /// Removes the specified item from the list.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    public void Remove(object item)
    {
        var isReadOnly = ((IList)Items).IsReadOnly;

        if (!isReadOnly)
            Items.Remove(item);
        else EditableItems.Remove(item);
    }

    /// <summary>
    /// Raises the <see cref="AddingNewItem"/> event.
    /// </summary>
    /// <param name="e">Event data for adding a new item.</param>
    protected virtual void OnAddingNewItem(AddingNewItemEventArgs e)
    {
        AddingNewItem?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="InitializingNewItem"/> event.
    /// </summary>
    /// <param name="e">Event data for initializing a new item.</param>
    protected virtual void OnInitializingNewItem(InitializingNewItemEventArgs e)
    {
        InitializingNewItem?.Invoke(this, e);
    }

    /// <inheritdoc/> 
    public void Serialize(XmlDocument doc, XmlNode parentNode)
    {
    }

    /// <inheritdoc/> 
    public void Deserialize(LayoutManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
    }

    #endregion
}
