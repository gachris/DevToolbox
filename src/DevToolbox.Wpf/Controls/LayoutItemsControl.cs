using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Serialization;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A tabbed control representing a dockable document pane.
/// Supports dragging tabs to float windows or dock within a <see cref="DockManager"/>, and layout serialization.
/// </summary>
public class LayoutItemsControl : TabControlEdit, IDropSurface, ILayoutSerializable
{
    #region Fields/Consts

    private LayoutItem? _draggedTab;
    private int? _originalIndex;
    private int? _lastSwapTargetIndex;
    private Point _ptFloatingWindow = new(0, 0);
    private Size _sizeFloatingWindow = new(300, 300);

    /// <summary>
    /// Occurs when the <see cref="Dock"/> property has changed.
    /// </summary>
    public event EventHandler<LayoutDockChangedEventArgs>? DockChanged;

    /// <summary>
    /// Occurs when the <see cref="State"/> property has changed.
    /// </summary>
    public event EventHandler<LayoutItemStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Identifies the <see cref="PaneWidth"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PaneWidthProperty =
        DependencyProperty.Register(nameof(PaneWidth), typeof(double), typeof(LayoutItemsControl), new PropertyMetadata(250D));

    /// <summary>
    /// Identifies the <see cref="PaneHeight"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PaneHeightProperty =
        DependencyProperty.Register(nameof(PaneHeight), typeof(double), typeof(LayoutItemsControl), new PropertyMetadata(250D));

    /// <summary>
    /// Identifies the <see cref="Dock"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register(nameof(Dock), typeof(Dock), typeof(LayoutItemsControl), new FrameworkPropertyMetadata(Dock.Right, OnDockChanged));

    /// <summary>
    /// Identifies the <see cref="State"/> dependency property key.
    /// </summary>
    private static readonly DependencyPropertyKey StatePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(State), typeof(LayoutItemState), typeof(LayoutItemsControl), new FrameworkPropertyMetadata(LayoutItemState.Document, OnStateChanged));

    /// <summary>
    /// Identifies the <see cref="State"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty StateProperty = StatePropertyKey.DependencyProperty;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the <see cref="DockManager"/> that contains this control.
    /// </summary>
    internal LayoutManager? DockManager { get; set; }

    /// <inheritdoc/>
    public Rect SurfaceRectangle => IsHidden ? new Rect() : new Rect(PointToScreen(new Point(0, 0)), new Size(ActualWidth, ActualHeight));

    /// <summary>
    /// Return true if control is hidden, ie State is different from PaneState.Document
    /// </summary>
    public bool IsHidden => State != LayoutItemState.Document;

    /// <summary>
    /// Gets or sets the default width of the pane when docked left or right.
    /// </summary>
    public double PaneWidth
    {
        get => (double)GetValue(PaneWidthProperty);
        set => SetValue(PaneWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the default height of the pane when docked top or bottom.
    /// </summary>
    public double PaneHeight
    {
        get => (double)GetValue(PaneHeightProperty);
        set => SetValue(PaneHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets which side of the <see cref="DockManager"/> this pane is docked to.
    /// </summary>
    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    /// <summary>
    /// Gets the current state of this pane (docking, windowed, document, auto-hide, etc.).
    /// </summary>
    public LayoutItemState State
    {
        get => (LayoutItemState)GetValue(StateProperty);
        internal set => SetValue(StatePropertyKey, value);
    }

    #endregion

    static LayoutItemsControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutItemsControl), new FrameworkPropertyMetadata(typeof(LayoutItemsControl)));
    }

    /// <summary>
    /// Initializes a new instance of <see cref="LayoutItemsControl"/>.
    /// </summary>
    public LayoutItemsControl()
    {
        ItemContainerGenerator.StatusChanged += OnItemGeneratorStatusChanged;
        Closed += LayoutItemClosed;
    }

    #region Methods Override

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DockManager ??= this.FindVisualAncestor<LayoutManager>();

        if (DockManager == null && TemplatedParent != null)
            DockManager = TemplatedParent.FindVisualAncestor<LayoutManager>();

        if (DockManager == null && VisualParent != null)
            DockManager = VisualParent.FindVisualAncestor<LayoutManager>();

        if (DockManager == null)
            throw new InvalidOperationException($"{nameof(TemplatedParent)} or {nameof(VisualParent)} of {typeof(LayoutDockItemsControl)} must be {typeof(LayoutManager)} type");
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride() => new LayoutItem();

    /// <inheritdoc/>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        SaveSize();
        base.OnRenderSizeChanged(sizeInfo);
    }

    #endregion

    #region Methods

    private static void OnDockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dockableControl = (LayoutItemsControl)d;
        dockableControl.OnDockChanged((Dock)e.OldValue, (Dock)e.NewValue);
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dockableControl = (LayoutItemsControl)d;
        dockableControl.OnStateChanged((LayoutItemState)e.OldValue, (LayoutItemState)e.NewValue);
    }

    private void OnDockChanged(Dock oldValue, Dock newValue)
    {
        SaveSize();
        DockChanged?.Invoke(this, new LayoutDockChangedEventArgs(oldValue, newValue));
    }

    private void OnStateChanged(LayoutItemState oldValue, LayoutItemState newValue)
    {
        SaveSize();
        StateChanged?.Invoke(this, new LayoutItemStateChangedEventArgs(oldValue, newValue));
    }

    /// <inheritdoc/>
    internal void SaveSize()
    {
        if (IsHidden)
        {
            return;
        }

        var paneWidthDefaultValue = (double)PaneWidthProperty.DefaultMetadata.DefaultValue;
        var paneHeightDefaultValue = (double)PaneHeightProperty.DefaultMetadata.DefaultValue;

        if (Dock is Dock.Left or Dock.Right)
        {
            PaneWidth = ActualWidth > paneWidthDefaultValue ? ActualWidth : paneWidthDefaultValue;
        }
        else
        {
            PaneHeight = ActualHeight > paneHeightDefaultValue ? ActualHeight : paneHeightDefaultValue;
        }
    }

    internal void SaveWindowSizeAndPosition(Window fw)
    {
        if (!double.IsNaN(fw.Left) && !double.IsNaN(fw.Top))
        {
            _ptFloatingWindow = new Point(fw.Left, fw.Top);
        }

        if (!double.IsNaN(fw.Width) && !double.IsNaN(fw.Height))
        {
            _sizeFloatingWindow = new Size(fw.Width, fw.Height);
        }
    }

    internal void RestoreWindowSizeAndPosition(Window window)
    {
        window.Left = _ptFloatingWindow.X;
        window.Top = _ptFloatingWindow.Y;
        window.Width = _sizeFloatingWindow.Width;
        window.Height = _sizeFloatingWindow.Height;
    }

    /// <inheritdoc/>
    public virtual void OnDragEnter(Point point)
    {
        DockManager?.LayoutDockTargetControl?.OnDragEnter(this, point);
    }

    /// <inheritdoc/>
    public virtual void OnDragOver(Point point)
    {
        DockManager?.LayoutDockTargetControl?.OnDragOver(this, point);
    }

    /// <inheritdoc/>
    public virtual void OnDragLeave(Point point)
    {
        DockManager?.LayoutDockTargetControl?.OnDragLeave(this, point);
    }

    /// <inheritdoc/>
    public virtual bool OnDrop(Point point) => false;

    private void DragContent(DependencyObject element, object item, Point startDragPoint, Point offset)
    {
        //if (element is LayoutItem documentItem && documentItem.IsDockable)
        //    CreateDockableHost(item, startDragPoint, offset);
        //else
        CreateDocumentHost(item, startDragPoint, offset);
    }

    private void CreateDockableHost(object item, Point startDragPoint, Point offset)
    {
        if (DockManager is null || State == LayoutItemState.Window) return;
        var window = DockManager.GetContainerForDockingOverride();

        Remove(item);

        if (Items.Count == 0)
        {
            DockManager.RemoveLayoutItemsControl(this);
        }

        LayoutDockItemsControl? newElement;
        var isReadOnlyDockManager = ((IList)DockManager.Items).IsReadOnly;

        if (isReadOnlyDockManager)
        {
            var newItem = DockManager.Add();
            newElement = DockManager.ContainerFromItem(newItem) as LayoutDockItemsControl;
        }
        else
        {
            newElement = DockManager.Add() as LayoutDockItemsControl;
        }

        if (newElement is null)
        {
            throw new InvalidOperationException("Failed to create a new LayoutDockItemsControl instance.");
        }

        newElement.Add(item);
        newElement.DockManager = DockManager;
        newElement.State = LayoutItemState.Window;
        window.SetHostContent(newElement);
        window.Owner = DockManager.Owner;
        RestoreWindowSizeAndPosition(window);
        DockManager.Drag(window, startDragPoint, offset);
    }

    private void CreateDocumentHost(object item, Point startDragPoint, Point offset)
    {
        if (DockManager is null || State == LayoutItemState.Window) return;
        var window = DockManager.GetContainerForDocumentItemOverride();

        Remove(item);

        if (Items.Count == 0)
        {
            DockManager.RemoveLayoutItemsControl(this);
        }

        var newElement = DockManager.AddNewLayoutItemsControl();
        newElement.Add(item);
        newElement.DockManager = DockManager;
        newElement.State = LayoutItemState.Window;
        window.SetHostContent(newElement);
        window.Owner = DockManager.Owner;
        RestoreWindowSizeAndPosition(window);
        DockManager.Drag(window, startDragPoint, offset);
    }

    /// <inheritdoc/>
    public virtual void Serialize(XmlDocument doc, XmlNode parentNode)
    {
        if (doc == null) throw new ArgumentNullException(nameof(doc));
        if (parentNode == null) throw new ArgumentNullException(nameof(parentNode));
        if (parentNode.Attributes == null) throw new ArgumentNullException(nameof(parentNode.Attributes));

        // Optional: store current size
        var widthAttr = doc.CreateAttribute("Width");
        widthAttr.Value = ActualWidth.ToString(CultureInfo.InvariantCulture);
        parentNode.Attributes.Append(widthAttr);

        var heightAttr = doc.CreateAttribute("Height");
        heightAttr.Value = ActualHeight.ToString(CultureInfo.InvariantCulture);
        parentNode.Attributes.Append(heightAttr);

        // Serialize each item by its type name
        foreach (var content in Items)
        {
            var typeString = content.GetType().FullName ?? content.GetType().Name;
            var itemNode = doc.CreateElement(typeString);
            parentNode.AppendChild(itemNode);
        }
    }

    /// <inheritdoc/>
    public virtual void Deserialize(LayoutManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
        if (node == null) throw new ArgumentNullException(nameof(node));
        if (getObjectHandler == null) throw new ArgumentNullException(nameof(getObjectHandler));

        // Re-attach the dock manager so PrepareContainerForItemOverride runs
        DockManager = managerToAttach ?? throw new ArgumentNullException(nameof(managerToAttach));

        // (Optional) restore size
        if (double.TryParse(node.Attributes?["Width"]?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var w))
            Width = w;
        if (double.TryParse(node.Attributes?["Height"]?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var h))
            Height = h;

        // Recreate each item by name and add it
        foreach (XmlNode childNode in node.ChildNodes)
        {
            var item = getObjectHandler(childNode.Name);
            Add(item);
        }
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Called when the ItemContainerGenerator changes status. Attaches drag handlers
    /// to each tab item's header.
    /// </summary>
    private void OnItemGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            return;

        foreach (var item in Items)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is LayoutItem tab)
            {
                tab.PreviewMouseLeftButtonDown -= Tab_PreviewMouseLeftButtonDown;
                tab.PreviewMouseMove -= Tab_PreviewMouseMove;
                tab.PreviewMouseLeftButtonUp -= Tab_PreviewMouseLeftButtonUp;

                tab.PreviewMouseLeftButtonDown += Tab_PreviewMouseLeftButtonDown;
                tab.PreviewMouseMove += Tab_PreviewMouseMove;
                tab.PreviewMouseLeftButtonUp += Tab_PreviewMouseLeftButtonUp;
            }
        }
    }

    /// <summary>
    /// Handles cleanup when a hosted layout item is closed. Removes the control from its group and closes the host if no items remain.
    /// </summary>
    /// <param name="sender">The document control that was closed.</param>
    /// <param name="e">Event arguments.</param>
    private void LayoutItemClosed(object? sender, EventArgs e)
    {
        if (State is LayoutItemState.Window || Items.Count > 0)
            return;

        var items = (IList)DockManager!.LayoutGroupItems.Items;
        if (items.IsReadOnly)
        {
            var item = DockManager.ItemFromContainer(this);
            DockManager.LayoutGroupItems.Remove(item);
        }
        else
        {
            DockManager.LayoutGroupItems.Remove(this);
        }
    }

    private void Tab_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is LayoutItem tab)
        {
            _draggedTab = tab;
            _originalIndex = Items.IndexOf(tab.DataContext ?? tab);
        }
    }

    private void Tab_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_draggedTab == null
            || e.LeftButton != MouseButtonState.Pressed
            || !_originalIndex.HasValue)
            return;

        var point = e.GetPosition(this);
        var element = InputHitTest(point) as DependencyObject;
        var hoveredElement = element.VisualUpwardSearch<Button>();
        if (hoveredElement is not null)
        {
            return;
        }

        Mouse.Capture(_draggedTab, CaptureMode.SubTree);

        var tabPanel = _draggedTab.VisualUpwardSearch<Panel>();
        if (tabPanel != null)
        {
            const int margin = 12;
            var relPos = e.GetPosition(tabPanel);
            if (relPos.X <= -margin
                || relPos.X >= tabPanel.ActualWidth + margin
                || relPos.Y <= -margin
                || relPos.Y >= tabPanel.ActualHeight + margin)
            {
                var screenPos = PointToScreen(point);
                var offset = e.GetPosition(_draggedTab);
                var item = _draggedTab.DataContext ?? _draggedTab;
                DragContent(_draggedTab, item, screenPos, offset);
                _draggedTab = null;
                _originalIndex = null;
                _lastSwapTargetIndex = null;
                return;
            }
        }

        var hoveredTab = element.VisualUpwardSearch<LayoutItem>();
        if (hoveredTab != null && hoveredTab != _draggedTab)
        {
            var sourceIndex = _originalIndex.Value;
            var targetIndex = Items.IndexOf(hoveredTab.DataContext ?? hoveredTab);

            if (targetIndex == _lastSwapTargetIndex)
                return;

            if (sourceIndex != targetIndex && sourceIndex >= 0 && targetIndex >= 0)
            {
                _originalIndex = targetIndex;

                var items = ((IList)Items).IsReadOnly && ItemsSource is IList src ? src : Items;
                var item = items[sourceIndex];
                items.RemoveAt(sourceIndex);
                items.Insert(targetIndex, item);
                SelectedIndex = targetIndex;
                _lastSwapTargetIndex = sourceIndex;
                _draggedTab = ItemContainerGenerator.ContainerFromIndex(targetIndex) as LayoutItem;
            }
        }
        else
        {
            _lastSwapTargetIndex = null;
        }
    }

    private void Tab_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_draggedTab?.IsMouseCaptured == true)
        {
            Mouse.Capture(null);
        }

        _draggedTab = null;
        _originalIndex = null;
        _lastSwapTargetIndex = null;
    }

    #endregion
}
