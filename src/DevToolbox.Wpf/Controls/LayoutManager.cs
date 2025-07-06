using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Serialization;
using DevToolbox.Wpf.Services;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Manages layout of dockable and document items within a container,
/// providing drag-and-drop docking, auto-hide panes, and serialization support.
/// </summary>
[TemplatePart(Name = PART_LeftPanel, Type = typeof(LayoutDockButtonGroupControl))]
[TemplatePart(Name = PART_RightPanel, Type = typeof(LayoutDockButtonGroupControl))]
[TemplatePart(Name = PART_TopPanel, Type = typeof(LayoutDockButtonGroupControl))]
[TemplatePart(Name = PART_BottomPanel, Type = typeof(LayoutDockButtonGroupControl))]
[TemplatePart(Name = PART_DockableOverlayControl, Type = typeof(LayoutDockOverlayControl))]
[TemplatePart(Name = PART_DockingPanel, Type = typeof(LayoutDockGroupPanel))]
public class LayoutManager : ItemsControl, IDropSurface
{
    #region Fields/Consts

    private bool _dragStarted;

    private const string PART_LeftPanel = nameof(PART_LeftPanel);
    private const string PART_RightPanel = nameof(PART_RightPanel);
    private const string PART_TopPanel = nameof(PART_TopPanel);
    private const string PART_BottomPanel = nameof(PART_BottomPanel);
    private const string PART_DockableOverlayControl = nameof(PART_DockableOverlayControl);
    private const string PART_DockingPanel = nameof(PART_DockingPanel);

    private readonly DockingDragServices _dragServices = new();
    private Window? _overlayWindow;

    private Window? _owner;
    private LayoutDockOverlayControl? _overlayControl;
    private LayoutDockGroupPanel? _dockingPanel;
    private LayoutDockButtonGroupControl? _topDockingButtonGroupControl;
    private LayoutDockButtonGroupControl? _leftDockingButtonGroupControl;
    private LayoutDockButtonGroupControl? _rightDockingButtonGroupControl;
    private LayoutDockButtonGroupControl? _bottomDockingButtonGroupControl;
    private LayoutDockTargetControl? _layoutDockTargetControl;

    /// <summary>
    /// Backing field for the <see cref="LayoutGroupItems"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty LayoutGroupItemsProperty =
        DependencyProperty.Register(
            nameof(LayoutGroupItems),
            typeof(LayoutGroupItemsControl),
            typeof(LayoutManager),
            new FrameworkPropertyMetadata(null, OnLayoutGroupItemsChanged));

    /// <summary>
    /// Dependency property for styling auto-hide overlay buttons.
    /// </summary>
    public static readonly DependencyProperty OverlayButtonStyleProperty =
        DependencyProperty.Register(
            nameof(OverlayButtonStyle),
            typeof(Style),
            typeof(LayoutManager),
            new FrameworkPropertyMetadata(default(Style)));

    /// <summary>
    /// Dependency property for styling dock buttons.
    /// </summary>
    public static readonly DependencyProperty DockButtonStyleProperty =
        DependencyProperty.Register(
            nameof(DockButtonStyle),
            typeof(Style),
            typeof(LayoutManager),
            new FrameworkPropertyMetadata(default(Style)));

    /// <summary>
    /// Event raised after a new item is created, for initialization.
    /// </summary>
    public event InitializingNewItemEventHandler? InitializingNewItem;

    /// <summary>
    /// Event raised before a new item is created.
    /// </summary>
    public event EventHandler<AddingNewItemEventArgs>? AddingNewItem;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the editable collection.
    /// </summary>
    private IEditableCollectionView EditableItems => Items;

    /// <summary>
    /// Gets the currently focused auto-hide dock button.
    /// </summary>
    public LayoutDockButton? FocusedDockingButton { get; private set; }

    /// <summary>
    /// Gets the overlay window used for docking previews.
    /// </summary>
    internal Window? OverlayWindow => _overlayWindow;

    /// <summary>
    /// Gets the active layout dock target control.
    /// </summary>
    internal LayoutDockTargetControl? LayoutDockTargetControl => _layoutDockTargetControl;

    /// <summary>
    /// Gets the screen rectangle occupied by this surface.
    /// </summary>
    public Rect SurfaceRectangle => new(PointToScreen(new(0, 0)), new Size(ActualWidth, ActualHeight));

    /// <summary>
    /// Gets the drag-and-drop services instance.
    /// </summary>
    internal DockingDragServices DragServices => _dragServices;

    /// <summary>
    /// Gets the owner window of this control.
    /// </summary>
    internal Window? Owner => _owner;

    /// <summary>
    /// Gets or sets the <see cref="LayoutGroupItemsControl"/> that provides document hosting.
    /// </summary>
    public LayoutGroupItemsControl LayoutGroupItems
    {
        get => (LayoutGroupItemsControl)GetValue(LayoutGroupItemsProperty);
        set => SetValue(LayoutGroupItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the style applied to overlay buttons.
    /// </summary>
    public Style OverlayButtonStyle
    {
        get => (Style)GetValue(OverlayButtonStyleProperty);
        set => SetValue(OverlayButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style applied to dock buttons.
    /// </summary>
    public Style DockButtonStyle
    {
        get => (Style)GetValue(DockButtonStyleProperty);
        set => SetValue(DockButtonStyleProperty, value);
    }

    #endregion

    static LayoutManager()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutManager), new FrameworkPropertyMetadata(typeof(LayoutManager)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutManager"/> class.
    /// </summary>
    public LayoutManager()
    {
        DragServices.Register(this);

        AddHandler(PreviewMouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
        AddHandler(PreviewMouseMoveEvent, new MouseEventHandler(OnMouseMove));

        CommandBindings.Add(new(LayoutDockButton.ShowHideOverlayCommand, ShowOverlayCommandExecute, ShowOverlayCommandCanExecute));
    }

    #region Methods Override

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _layoutDockTargetControl = new LayoutDockTargetControl(this);

        _overlayWindow?.Close();
        _overlayWindow = new Window()
        {
            ShowActivated = false,
            AllowsTransparency = true,
            WindowStyle = WindowStyle.None,
            Background = System.Windows.Media.Brushes.Transparent,
            ShowInTaskbar = false,
            Content = _layoutDockTargetControl
        };
        _overlayWindow.Hide();

        if (_owner != null)
            _owner.Closed -= WindowClosed;

        _owner = Window.GetWindow(this);

        if (_owner != null)
            _owner.Closed += WindowClosed;

        _leftDockingButtonGroupControl = Template.FindName(PART_LeftPanel, this) as LayoutDockButtonGroupControl;
        _rightDockingButtonGroupControl = Template.FindName(PART_RightPanel, this) as LayoutDockButtonGroupControl;
        _topDockingButtonGroupControl = Template.FindName(PART_TopPanel, this) as LayoutDockButtonGroupControl;
        _bottomDockingButtonGroupControl = Template.FindName(PART_BottomPanel, this) as LayoutDockButtonGroupControl;

        _dockingPanel?.RemoveHandler(MouseDownEvent, new MouseButtonEventHandler(OnHideAutoHidePane));

        _dockingPanel = Template.FindName(PART_DockingPanel, this) as LayoutDockGroupPanel;

        _dockingPanel?.AddHandler(MouseDownEvent, new MouseButtonEventHandler(OnHideAutoHidePane), true);
        _dockingPanel?.AttachDocumentList(LayoutGroupItems);

        _overlayControl = Template.FindName(PART_DockableOverlayControl, this) as LayoutDockOverlayControl;
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is LayoutDockItemsControl dockableControl)
        {
            dockableControl.StateChanged += OnStateChanged;
            dockableControl.DockChanged += OnDockChanged;

            if (dockableControl.State == LayoutItemState.Docking)
                DragServices.Register(dockableControl);

            _dockingPanel?.Add(dockableControl, dockableControl.Dock);
        }
    }

    /// <inheritdoc/>
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is LayoutDockItemsControl dockableControl)
        {
            dockableControl.StateChanged -= OnStateChanged;
            dockableControl.DockChanged -= OnDockChanged;

            _dockingPanel?.Remove(dockableControl);
            DragServices.Unregister(dockableControl);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new LayoutDockItemsControl();
    }

    #endregion

    #region Methods

    private static void OnLayoutGroupItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dockManager = (LayoutManager)d;
        dockManager.OnLayoutGroupItemsChanged(e);
    }

    private void OnLayoutGroupItemsChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not LayoutGroupItemsControl documentList) return;

        documentList.AttachDockManager(this);
        _dockingPanel?.AttachDocumentList(documentList);
    }

    /// <summary>
    /// Creates or identifies the window that is used to display the given docking item.
    /// </summary>
    /// <returns>The window that is used to display the given docking item.</returns>
    protected internal virtual LayoutDockWindow GetContainerForDockingOverride()
    {
        return new();
    }

    /// <summary>
    /// Creates or identifies the window that is used to display the given document item.
    /// </summary>
    /// <returns>The window that is used to display the given document item.</returns>
    protected internal virtual LayoutWindow GetContainerForDocumentItemOverride()
    {
        return new();
    }

    internal void MoveTo(LayoutItemsControl control, LayoutItemsControl relativeControl, Dock relativeDock)
    {
        control.Dock = relativeControl.Dock;
        control.State = LayoutItemState.Document;
        LayoutGroupItems.LayoutGroupPanel?.Remove(control);
        LayoutGroupItems.LayoutGroupPanel?.Add(control, relativeControl, relativeDock);
    }

    internal void MoveTo(LayoutDockItemsControl control, LayoutItemsControl relativeControl, Dock relativeDock)
    {
        var newElement = AddNewLayoutItemsControl();

        while (control.Items.Count > 0)
        {
            var item = control.Items[0];
            control.Remove(item);
            newElement.Add(item);

            var element = newElement.ContainerFromItem(item) ?? item;
            if (element is LayoutItem documentItem)
            {
                //documentItem.IsDockable = true;
            }
        }

        RemoveLayoutDockItemsControl(control);

        newElement.Dock = relativeControl.Dock;
        newElement.State = LayoutItemState.Docking;
        LayoutGroupItems.LayoutGroupPanel?.Remove(newElement);
        LayoutGroupItems.LayoutGroupPanel?.Add(newElement, relativeControl, relativeDock);
    }

    internal void MoveTo(LayoutDockItemsControl control, LayoutDockItemsControl relativeControl, Dock relativeDock)
    {
        control.Dock = relativeControl.Dock;
        control.State = LayoutItemState.Docking;
        _dockingPanel?.Remove(control);
        _dockingPanel?.Add(control, relativeControl, relativeDock);
    }

    /// <summary>
    /// Move contained contents into a destination control and close this one
    /// </summary>
    internal void MoveInto(LayoutItemsControl control)
    {
        control.Dock = Dock.Left;
        control.State = LayoutItemState.Document;
        LayoutGroupItems.LayoutGroupPanel?.Remove(control);
        LayoutGroupItems.LayoutGroupPanel?.Add(control);
    }

    internal void MoveInto(LayoutItemsControl source, LayoutItemsControl destination)
    {
        while (source.Items.Count > 0)
        {
            var item = source.Items[0];
            source.Remove(item);
            destination.Add(item);
        }

        RemoveLayoutItemsControl(source);
    }

    internal void MoveInto(LayoutDockItemsControl source, LayoutItemsControl destination)
    {
        while (source.Items.Count > 0)
        {
            var item = source.Items[0];
            source.Remove(item);
            destination.Add(item);

            var element = destination.ContainerFromItem(item) ?? item;
            if (element is LayoutItem documentItem)
            {
                //documentItem.IsDockable = true;
            }
        }

        RemoveLayoutDockItemsControl(source);
    }

    /// <summary>
    /// Called from a control when it's dropped into an other control
    /// </summary>
    /// <param name="source">Source control which is going to be closed</param>
    /// <param name="destination">Destination control which is about to host contents from SourcePane</param>
    internal void MoveInto(LayoutDockItemsControl source, LayoutDockItemsControl destination)
    {
        while (source.Items.Count > 0)
        {
            var item = source.Items[0];
            source.Remove(item);
            destination.Add(item);
        }

        RemoveLayoutDockItemsControl(source);
    }

    internal LayoutDockItemsControl AddNewLayoutDockItemsControl()
    {
        var items = (IList)Items;
        LayoutDockItemsControl? newElement;

        if (items.IsReadOnly)
        {
            var newItem = Add();
            newElement = this.ContainerFromItem(newItem) as LayoutDockItemsControl;
        }
        else
        {
            newElement = Add() as LayoutDockItemsControl;
        }

        if (newElement is null)
        {
            throw new InvalidOperationException(
                "Failed to create a new LayoutDockItemsControl instance. Add() returned null."
            );
        }

        return newElement;
    }

    internal LayoutItemsControl AddNewLayoutItemsControl()
    {
        var items = (IList)LayoutGroupItems.Items;
        LayoutItemsControl? newElement;

        if (items.IsReadOnly)
        {
            var newItem = LayoutGroupItems.Add();
            newElement = LayoutGroupItems.ContainerFromItem(newItem) as LayoutItemsControl;
        }
        else
        {
            newElement = LayoutGroupItems.Add() as LayoutItemsControl;
        }

        if (newElement is null)
        {
            throw new InvalidOperationException(
                "Failed to create a new LayoutItemsControl instance. LayoutGroupItems.Add() returned null."
            );
        }

        return newElement;
    }

    internal void RemoveLayoutDockItemsControl(LayoutDockItemsControl control)
    {
        var isReadOnly = ((IList)Items).IsReadOnly;
        if (isReadOnly)
        {
            var item = this.ItemFromContainer(control);
            Remove(item);
        }
        else
        {
            Remove(control);
        }
    }

    internal void RemoveLayoutItemsControl(LayoutItemsControl control)
    {
        var isReadOnly = ((IList)LayoutGroupItems.Items).IsReadOnly;
        if (isReadOnly)
        {
            var item = LayoutGroupItems.ItemFromContainer(control);
            LayoutGroupItems.Remove(item);
        }
        else
        {
            LayoutGroupItems.Remove(control);
        }
    }

    /// <summary>
    /// Add a group of docking buttons for a control docked to a docking manager border
    /// </summary>
    /// <param name="dockableControl"></param>
    private void AddDockingButtons(LayoutDockItemsControl dockableControl)
    {
        var dockingButtonGroupItem = new LayoutDockButtonGroupItem() { Dock = dockableControl.Dock };

        foreach (var item in dockableControl.Items)
        {
            var index = dockableControl.Items.IndexOf(item);
            dockableControl.UpdateLayout();

            if (item is not LayoutDockItem containerItem)
                containerItem = (LayoutDockItem)dockableControl.ItemContainerGenerator.ContainerFromItem(item);

            if (containerItem == null)
                throw new ArgumentNullException(nameof(containerItem));

            var dockingButtonItem = new LayoutDockButton()
            {
                Icon = containerItem.Icon,
                Content = containerItem.Header,
                AssociatedItemIndex = index,
                AssociatedContainer = dockableControl,
                ParentGroupItem = dockingButtonGroupItem
            };

            dockingButtonGroupItem.Items.Add(dockingButtonItem);
        }

        switch (dockingButtonGroupItem.Dock)
        {
            case Dock.Left:
                _leftDockingButtonGroupControl?.Items.Add(dockingButtonGroupItem);
                break;
            case Dock.Right:
                _rightDockingButtonGroupControl?.Items.Add(dockingButtonGroupItem);
                break;
            case Dock.Top:
                _topDockingButtonGroupControl?.Items.Add(dockingButtonGroupItem);
                break;
            case Dock.Bottom:
                _bottomDockingButtonGroupControl?.Items.Add(dockingButtonGroupItem);
                break;
        }
    }

    /// <summary>
    /// Handle AutoHide/Hide command issued by user on temporary control
    /// </summary>
    /// <param name="dockableControl"></param>
    private void RemoveDockingButtons(LayoutDockItemsControl dockableControl)
    {
        if (FocusedDockingButton == null)
            return;

        switch (FocusedDockingButton.Dock)
        {
            case Dock.Left:
                _leftDockingButtonGroupControl?.Items.Remove(FocusedDockingButton.ParentGroupItem);
                break;
            case Dock.Right:
                _rightDockingButtonGroupControl?.Items.Remove(FocusedDockingButton.ParentGroupItem);
                break;
            case Dock.Top:
                _topDockingButtonGroupControl?.Items.Remove(FocusedDockingButton.ParentGroupItem);
                break;
            case Dock.Bottom:
                _bottomDockingButtonGroupControl?.Items.Remove(FocusedDockingButton.ParentGroupItem);
                break;
        }

        _overlayControl?.Hide(FocusedDockingButton);
    }

    /// <summary>
    /// Event handler which show a temporary control with a single content attached to a docking button
    /// </summary>
    /// <param name="dockingButton"></param>
    private void OnShowAutoHidePane(LayoutDockButton dockingButton)
    {
        if (FocusedDockingButton == dockingButton)
        {
            _overlayControl?.Hide(FocusedDockingButton);
            FocusedDockingButton = null;
            return;
        }

        if (FocusedDockingButton is not null)
        {
            _overlayControl?.Hide(FocusedDockingButton);
        }

        FocusedDockingButton = dockingButton;

        _overlayControl?.Show(FocusedDockingButton);
    }

    /// <summary>
    /// Begins dragging operations
    /// </summary>
    /// <param name="window">Floating window containing control which is dragged by user</param>
    /// <param name="point">Current mouse position</param>
    /// <param name="offset">Offset to be use to set floating window screen position</param>
    /// <returns>Returns True is drag is completed, false otherwise</returns>
    public bool Drag(LayoutBaseWindow window, Point point, Point offset)
    {
        if (!IsMouseCaptured)
        {
            if (CaptureMouse())
            {
                window.Owner = Owner;
                DragServices.StartDrag(window, point, offset);
                _dragStarted = true;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Adds a new item to the internal collection, creating and initializing its container.
    /// </summary>
    /// <returns>The newly created item container.</returns>
    public object Add()
    {
        return AddNewItem();
    }

    /// <summary>
    /// Adds the specified item to the internal collection, initializing its container.
    /// </summary>
    /// <param name="item">The existing item to add.</param>
    /// <returns>The created or added item container.</returns>
    public object Add(object item)
    {
        return AddNewItem(item);
    }

    /// <summary>
    /// Core logic for adding new items, handling both readonly and writable collection scenarios.
    /// </summary>
    /// <param name="item">
    /// An existing item to add; if <c>null</c>, a new container will be created.
    /// </param>
    /// <returns>The newly added or created item container.</returns>
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
    /// Removes the specified item from the internal collection, supporting both readonly and writable sources.
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

    /// <summary>
    /// Serialize layout state of panes and contents into a xml string
    /// </summary>
    /// <returns>Xml containing layout state</returns>
    public string GetLayoutAsXml()
    {
        var doc = new XmlDocument();

        doc.AppendChild(doc.CreateElement("DockingLibrary_Layout"));

        if (doc.DocumentElement is not null)
        {
            _dockingPanel?.Serialize(doc, doc.DocumentElement);
        }

        return doc.OuterXml;
    }

    /// <summary>
    /// Restore docking layout reading a xml string which is previously generated by a call to GetLayoutState
    /// </summary>
    /// <param name="xml">Xml containing layout state</param>
    /// <param name="getContentHandler">Delegate used by serializer to get user defined dockable contents</param>
    public void RestoreLayoutFromXml(string xml, GetContentFromTypeString getContentHandler)
    {
        XmlDocument doc = new();
        doc.LoadXml(xml);

        _dockingPanel?.Deserialize(this, doc.ChildNodes[0]!, getContentHandler);

        var addedControls = new List<LayoutDockItemsControl>();
        foreach (LayoutDockItemsControl content in Items)
        {
            if (!addedControls.Contains(content))
            {
                if (content.State == LayoutItemState.AutoHide)
                {
                    addedControls.Add(content);
                    AddDockingButtons(content);
                }
            }
        }

        FocusedDockingButton = null;
    }

    #endregion

    #region IDropSurface

    /// <summary>
    /// Handles this surface mouse entering (show current overlay window)
    /// </summary>
    /// <param name="point">Current mouse position</param>
    public void OnDragEnter(Point point)
    {
        if (OverlayWindow is null)
        {
            throw new InvalidOperationException("OverlayWindow is not initialized. Ensure that the LayoutManager template is applied and the PART_DockableOverlayControl is defined.");
        }

        OverlayWindow.Owner = Owner;
        OverlayWindow.Left = PointToScreen(new Point(0, 0)).X;
        OverlayWindow.Top = PointToScreen(new Point(0, 0)).Y;
        OverlayWindow.Width = ActualWidth;
        OverlayWindow.Height = ActualHeight;
        OverlayWindow.Show();
    }

    /// <summary>
    /// Handles mouse overing this surface
    /// </summary>
    /// <param name="point"></param>
    public void OnDragOver(Point point)
    {
    }

    /// <summary>
    /// Handles mouse leave event during drag (hide overlay window)
    /// </summary>
    /// <param name="point"></param>
    public void OnDragLeave(Point point)
    {
        if (OverlayWindow is null)
        {
            throw new InvalidOperationException("OverlayWindow is not initialized. Ensure that the LayoutManager template is applied and the PART_DockableOverlayControl is defined.");
        }

        OverlayWindow.Owner = null;
        OverlayWindow.Hide();
    }

    /// <summary>
    /// Handler drop events
    /// </summary>
    /// <param name="point">Current mouse position</param>
    /// <returns>Returns always false because this surface doesn't support direct drop</returns>
    public bool OnDrop(Point point) => false;

    #endregion

    #region Events Subscriptions

    private void OnDockChanged(object? sender, LayoutDockChangedEventArgs e)
    {
        if (sender is not LayoutDockItemsControl dockable) return;

        _dockingPanel?.Remove(dockable);
        _dockingPanel?.Add(dockable, dockable.Dock);
    }

    /// <summary>
    /// Handles control state changed event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">An <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
    private void OnStateChanged(object? sender, LayoutItemStateChangedEventArgs e)
    {
        if (sender is not LayoutDockItemsControl content)
        {
            return;
        }

        if (e.NewValue == LayoutItemState.AutoHide)
        {
            AddDockingButtons(content);

            if (FocusedDockingButton is not null)
            {
                _overlayControl?.Show(FocusedDockingButton);
                _overlayControl?.Hide(FocusedDockingButton);
            }

            FocusedDockingButton = null;
        }

        if (e.OldValue == LayoutItemState.AutoHide)
        {
            RemoveDockingButtons(content);
            FocusedDockingButton = null;
        }

        _dockingPanel?.Remove(content);
        _dockingPanel?.Add(content, content.Dock);
    }

    /// <summary>
    /// Event handler which hide temporary control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnHideAutoHidePane(object sender, MouseButtonEventArgs e)
    {
        if (FocusedDockingButton is null)
        {
            return;
        }

        _overlayControl?.Hide(FocusedDockingButton);
        FocusedDockingButton = null;
    }

    /// <summary>
    /// Handles mouse move event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragStarted)
            DragServices.MoveDrag(PointToScreen(e.GetPosition(this)));
    }

    /// <summary>
    /// Handles mouseUp event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <remarks>Releases eventually captured mouse events</remarks>
    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (IsMouseCaptured)
        {
            _dragStarted = false;
            if (OverlayWindow is not null)
            {
                OverlayWindow.Owner = null;
            }

            DragServices.EndDrag(PointToScreen(e.GetPosition(this)));
            ReleaseMouseCapture();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    private void WindowClosed(object? sender, EventArgs e)
    {
        if (_owner is not null)
        {
            _owner.Closed -= WindowClosed;
        }

        _overlayWindow?.Close();
    }

    #endregion

    #region Commands

    private void ShowOverlayCommandExecute(object sender, ExecutedRoutedEventArgs e)
    {
        OnShowAutoHidePane((LayoutDockButton)e.OriginalSource);
    }

    private void ShowOverlayCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is LayoutManager && e.OriginalSource is LayoutDockButton;
    }

    #endregion
}
