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

public class DocumentList : Selector, ILayoutSerializable
{
    #region Fields/Consts

    private DockManager? _dockManager;
    private DocumentPanel? _documentPanel;

    /// <summary>
    ///     An event that is raised when a new item is created so that
    ///     developers can initialize the item with custom default values.
    /// </summary>
    public event InitializingNewItemEventHandler? InitializingNewItem;

    /// <summary>
    ///     An event that is raised before a new item is created so that
    ///     developers can participate in the construction of the new item.
    /// </summary>
    public event EventHandler<AddingNewItemEventArgs>? AddingNewItem;

    #endregion

    #region Properties

    private IEditableCollectionView EditableItems => Items;

    public Rect SurfaceRectangle => new(PointToScreen(new Point(0, 0)), new Size(ActualWidth, ActualHeight));

    #endregion

    static DocumentList()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DocumentList), new FrameworkPropertyMetadata(typeof(DocumentList)));
    }

    #region Methods Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _documentPanel = Template.FindName("PART_DocumentPanel", this) as DocumentPanel;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is DocumentControl documentControl)
        {
            if (documentControl.State == State.Document)
                _dockManager?.DragServices.Register(documentControl);

            documentControl.StateChanged += StateChanged;
            documentControl.DockChanged += OnDockChaged;

            _documentPanel?.Add(documentControl);
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is DocumentControl documentControl)
        {
            _dockManager?.DragServices.Unregister(documentControl);

            documentControl.DockChanged -= OnDockChaged;
            documentControl.StateChanged -= StateChanged;

            _documentPanel?.Remove(documentControl);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DocumentControl();
    }

    #endregion

    #region Methods

    internal void AttacheDockManager(DockManager dockManager)
    {
        _dockManager = dockManager;
    }

    private void OnDockChaged(object? sender, DockChangedEventArgs e)
    {
        _documentPanel?.ArrangeLayout();
    }

    private void StateChanged(object? sender, StateChangedEventArgs e)
    {
        if (sender is not IDropSurface dropSurface)
        {
            return;
        }

        if (e.NewValue is State.Document or State.Docking)
            _dockManager?.DragServices.Register(dropSurface);
        else
            _dockManager?.DragServices.Unregister(dropSurface);

        _documentPanel?.ArrangeLayout();
    }

    internal void MoveTo(DocumentControl control, DocumentControl relativeControl, Dock relativeDock)
    {
        control.State = State.Document;

        _documentPanel?.Remove(control);
        _documentPanel?.Add(control, relativeControl, relativeDock);
    }

    internal void MoveTo(DockableControl control, DocumentControl relativeControl, Dock relativeDock)
    {
        control.State = State.Document;
        //_documentPanel?.Remove(control);
        //_documentPanel?.Add(control, relativeControl, relativeDock);
    }

    public object Add()
    {
        return AddNewItem();
    }

    public object Add(object item)
    {
        return AddNewItem(item);
    }

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

    public void Remove(object item)
    {
        var isReadOnly = ((IList)Items).IsReadOnly;

        if (!isReadOnly)
            Items.Remove(item);
        else EditableItems.Remove(item);
    }

    /// <summary>
    ///     A method that is called before a new item is created so that
    ///     overrides can participate in the construction of the new item.
    /// </summary>
    /// <remarks>
    ///     The default implementation raises the AddingNewItem event.
    /// </remarks>
    /// <param name="e">Event arguments that provide access to the new item.</param>
    protected virtual void OnAddingNewItem(AddingNewItemEventArgs e)
    {
        AddingNewItem?.Invoke(this, e);
    }

    /// <summary>
    ///     A method that is called when a new item is created so that
    ///     overrides can initialize the item with custom default values.
    /// </summary>
    /// <remarks>
    ///     The default implementation raises the InitializingNewItem event.
    /// </remarks>
    /// <param name="e">Event arguments that provide access to the new item.</param>
    protected virtual void OnInitializingNewItem(InitializingNewItemEventArgs e)
    {
        InitializingNewItem?.Invoke(this, e);
    }

    public void Serialize(XmlDocument doc, XmlNode parentNode)
    {
    }

    public void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
    }

    #endregion
}