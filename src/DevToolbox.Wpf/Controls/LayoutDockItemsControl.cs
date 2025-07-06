using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Serialization;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A <see cref="LayoutDockItemsControl"/> that can be docked, floated, auto-hidden or hidden in a <see cref="DockManager"/>.
/// </summary>
[TemplatePart(Name = PART_Header, Type = typeof(ContentControl))]
public sealed class LayoutDockItemsControl : TabControlEdit, IDropSurface, ILayoutSerializable
{
    #region Fields/Consts

    private const string PART_Header = nameof(PART_Header);

    private LayoutDockItem? _draggedTab;
    private int? _originalIndex;
    private int? _lastSwapTargetIndex;
    private Point _headerDragStart;
    private ContentControl? _header;
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
    /// Identifies the <see cref="HeaderTemplate"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(LayoutDockItemsControl), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Identifies the <see cref="Header"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(LayoutDockItemsControl), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Identifies the <see cref="Icon"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(LayoutDockItemsControl), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Identifies the <see cref="PaneWidth"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PaneWidthProperty =
        DependencyProperty.Register(nameof(PaneWidth), typeof(double), typeof(LayoutDockItemsControl), new PropertyMetadata(250D));

    /// <summary>
    /// Identifies the <see cref="PaneHeight"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PaneHeightProperty =
        DependencyProperty.Register(nameof(PaneHeight), typeof(double), typeof(LayoutDockItemsControl), new PropertyMetadata(250D));

    /// <summary>
    /// Identifies the <see cref="Dock"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register(nameof(Dock), typeof(Dock), typeof(LayoutDockItemsControl), new FrameworkPropertyMetadata(Dock.Right, OnDockChanged));

    /// <summary>
    /// Identifies the <see cref="State"/> dependency property key.
    /// </summary>
    private static readonly DependencyPropertyKey StatePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(State), typeof(LayoutItemState), typeof(LayoutDockItemsControl), new FrameworkPropertyMetadata(LayoutItemState.Docking, OnStateChanged));

    /// <summary>
    /// Identifies the <see cref="ShowHeader"/> dependency property key.
    /// </summary>
    private static readonly DependencyPropertyKey ShowHeaderPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ShowHeader), typeof(bool), typeof(LayoutDockItemsControl), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Identifies the <see cref="State"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty StateProperty = StatePropertyKey.DependencyProperty;

    /// <summary>
    /// Identifies the <see cref="ShowHeader"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowHeaderProperty = ShowHeaderPropertyKey.DependencyProperty;

    #endregion

    #region Properties

    /// <summary>
    /// Command to float the selected tab into its own window.
    /// </summary>
    public static RoutedUICommand DockableWindowCommand { get; } =
        new(nameof(DockableWindowCommand), nameof(DockableWindowCommand), typeof(LayoutDockItemsControl));

    /// <summary>
    /// Command to dock the currently floating window back into the layout.
    /// </summary>
    public static RoutedUICommand DockCommand { get; } =
        new(nameof(Dock), nameof(Dock), typeof(LayoutDockItemsControl));

    /// <summary>
    /// Command to convert the selected pane into a tabbed document in the main document area.
    /// </summary>
    public static RoutedUICommand TabbedDocumentCommand { get; } =
        new(nameof(TabbedDocumentCommand), nameof(TabbedDocumentCommand), typeof(LayoutDockItemsControl));

    /// <summary>
    /// Command to toggle the auto-hide state of the pane (slide it in and out on hover).
    /// </summary>
    public static RoutedUICommand AutoHideCommand { get; } =
        new(nameof(AutoHideCommand), nameof(AutoHideCommand), typeof(LayoutDockItemsControl));

    /// <summary>
    /// Command to hide the pane completely until reactivated.
    /// </summary>
    public static RoutedUICommand HideCommand { get; } =
        new(nameof(HideCommand), nameof(HideCommand), typeof(LayoutDockItemsControl));

    /// <summary>
    /// Gets the <see cref="DockManager"/> that contains this control.
    /// </summary>
    internal LayoutManager? DockManager { get; set; }

    /// <inheritdoc/>
    public Rect SurfaceRectangle => IsHidden ? new Rect() : new Rect(PointToScreen(new Point(0, 0)), new Size(ActualWidth, ActualHeight));

    /// <summary>
    /// Return true if control is hidden, ie State is different from PaneState.Docked
    /// </summary>
    public bool IsHidden => State != LayoutItemState.Docking;

    /// <summary>
    /// Gets or sets the icon displayed in the header of the dockable pane.
    /// </summary>
    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the header content object.
    /// </summary>
    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display the header content.
    /// </summary>
    public DataTemplate HeaderTemplate
    {
        get => (DataTemplate)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

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

    /// <summary>
    /// Gets a value indicating whether the header is visible.
    /// </summary>
    public bool ShowHeader
    {
        get => (bool)GetValue(ShowHeaderProperty);
        private set => SetValue(ShowHeaderPropertyKey, value);
    }

    #endregion

    static LayoutDockItemsControl()
    {
        var typeFromHandle = typeof(LayoutDockItemsControl);

        DefaultStyleKeyProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(typeFromHandle));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
            new(DockableWindowCommand, (sender, e) => (sender as LayoutDockItemsControl)?.DockingWindowCommandExecute(e), (sender, e) => (sender as LayoutDockItemsControl)?.DockingWindowCommandCanExecute(e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
             new(DockCommand, (sender, e) => (sender as LayoutDockItemsControl)?.DockCommandExecute(e), (sender, e) => (sender as LayoutDockItemsControl)?.DockCommandCanExecute(e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
            new(TabbedDocumentCommand, (sender, e) => (sender as LayoutDockItemsControl)?.TabbedDocumentCommandExecute(e), (sender, e) => (sender as LayoutDockItemsControl)?.TabbedDocumentCommandCanExecute(e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
            new(AutoHideCommand, (sender, e) => (sender as LayoutDockItemsControl)?.AutoHideCommandExecute(e), (sender, e) => (sender as LayoutDockItemsControl)?.AutoHideCommandCanExecute(e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
            new(HideCommand, (sender, e) => (sender as LayoutDockItemsControl)?.HideCommandExecute(e), (sender, e) => (sender as LayoutDockItemsControl)?.HideCommandCanExecute(e)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutDockItemsControl"/> class,
    /// and hooks into item generator to wire up tab header drag events.
    /// </summary>
    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutDockItemsControl"/> class,
    /// and registers for the <see cref="ItemContainerGenerator.StatusChanged"/> event
    /// to hook up drag handlers on each tab header when containers are generated.
    /// </summary>
    public LayoutDockItemsControl()
    {
        ItemContainerGenerator.StatusChanged += OnItemGeneratorStatusChanged;
    }

    #region Methods Override

    /// <inheritdoc/>
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

        if (_header != null)
        {
            _header.RemoveHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Header_MouseDown));
            _header.RemoveHandler(MouseMoveEvent, new MouseEventHandler(Header_MouseMove));
            _header.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(Header_MouseUp));
        }

        _header = Template.FindName(PART_Header, this) as ContentControl;

        if (_header != null)
        {
            _header.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Header_MouseDown), false);
            _header.AddHandler(MouseMoveEvent, new MouseEventHandler(Header_MouseMove), false);
            _header.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(Header_MouseUp), false);
        }

        if (State == LayoutItemState.Docking)
        {
            DockManager.DragServices.Register(this);
        }
    }

    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new LayoutDockItem();
    }

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
        var dockableControl = (LayoutDockItemsControl)d;
        dockableControl.OnDockChanged((Dock)e.OldValue, (Dock)e.NewValue);
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dockableControl = (LayoutDockItemsControl)d;
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
        ShowHeader = newValue != LayoutItemState.Window;
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
    public void OnDragEnter(Point point)
    {
        DockManager?.LayoutDockTargetControl.OnDragEnter(this, point);
    }

    /// <inheritdoc/>
    public void OnDragOver(Point point)
    {
        DockManager?.LayoutDockTargetControl.OnDragOver(this, point);
    }

    /// <inheritdoc/>
    public void OnDragLeave(Point point)
    {
        DockManager?.LayoutDockTargetControl.OnDragLeave(this, point);
    }

    /// <inheritdoc/>
    public bool OnDrop(Point point)
    {
        return false;
    }

    /// <summary>
    /// Initiate a dragging operation of this control, relative DockManager is also involved
    /// </summary>
    /// <param name="startDragPoint"></param>
    /// <param name="offset"></param>
    private void DragContent(Point startDragPoint, Point offset)
    {
        if (DockManager is null || State == LayoutItemState.Window)
        {
            throw new InvalidOperationException("DockManager is null or the control is already in a floating window state.");
        }

        var window = DockManager.GetContainerForDockingOverride();

        State = LayoutItemState.Window;
        window.Content = this;
        window.Owner = DockManager.Owner;
        RestoreWindowSizeAndPosition(window);

        window.Show();
        DockManager.Drag(window, startDragPoint, offset);
    }

    private void CreateDockableHost(object item, Point startDragPoint, Point offset)
    {
        var window = CreateDockableHost(item);
        DockManager!.Drag(window, startDragPoint, offset);
    }

    /// <summary>
    /// Create and show a floating window hosting this control
    /// </summary> 
    private LayoutBaseWindow CreateDockableHost(object item)
    {
        if (DockManager is null)
        {
            throw new InvalidOperationException("DockManager is null or the control is already in a floating window state.");
        }

        var window = DockManager.GetContainerForDockingOverride();
        var isReadOnly = ((IList)DockManager.Items).IsReadOnly;

        Remove(item);

        if (Items.Count == 0)
        {
            if (!isReadOnly)
            {
                DockManager.Remove(this);
            }
            else
            {
                var currentItem = DockManager.ItemFromContainer(this);
                DockManager.Remove(currentItem);
            }
        }

        LayoutDockItemsControl? newElement;

        if (isReadOnly)
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
            throw new InvalidOperationException("Failed to create a new DockableControl instance.");
        }

        newElement.Add(item);
        newElement.DockManager = DockManager;
        newElement.State = LayoutItemState.Window;

        window.Content = newElement;
        window.Owner = DockManager.Owner;
        RestoreWindowSizeAndPosition(window);

        return window;
    }

    private void TabbedDocument(object item)
    {
        if (DockManager is null)
        {
            return;
        }

        Remove(item);

        if (Items.Count == 0)
        {
            var isReadOnly = ((IList)DockManager.Items).IsReadOnly;
            if (!isReadOnly)
            {
                DockManager.Remove(this);
            }
            else
            {
                var currentItem = DockManager.ItemFromContainer(this);
                DockManager.Remove(currentItem);
            }

            State = LayoutItemState.Hidden;
        }

        var documentControlElement = (LayoutItemsControl)DockManager.LayoutGroupItems.ContainerFromItem(DockManager.LayoutGroupItems.Items[0]);
        documentControlElement.Add(item);
    }

    /// <inheritdoc/>
    public void Serialize(XmlDocument doc, XmlNode parentNode)
    {
        // Ensure we capture the latest size before serializing
        SaveSize();

        // 1) Attributes for this control
        if (parentNode.Attributes is not null)
        {
            var a = doc.CreateAttribute("Dock");
            a.Value = Dock.ToString();
            parentNode.Attributes.Append(a);

            a = doc.CreateAttribute("State");
            a.Value = State.ToString();
            parentNode.Attributes.Append(a);

            a = doc.CreateAttribute("Width");
            a.Value = PaneWidth.ToString();
            parentNode.Attributes.Append(a);

            a = doc.CreateAttribute("Height");
            a.Value = PaneHeight.ToString();
            parentNode.Attributes.Append(a);

            a = doc.CreateAttribute("ptFloatingWindow");
            a.Value = TypeDescriptor.GetConverter(typeof(Point))
                                   .ConvertToInvariantString(_ptFloatingWindow);
            parentNode.Attributes.Append(a);

            a = doc.CreateAttribute("sizeFloatingWindow");
            a.Value = TypeDescriptor.GetConverter(typeof(Size))
                                   .ConvertToInvariantString(_sizeFloatingWindow);
            parentNode.Attributes.Append(a);
        }

        // 2) One child node per tab: use handler to re-create content later
        foreach (var item in Items)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is LayoutDockItem)
            {
                // We use the item's type name as the XML element name
                var node = doc.CreateElement(item.GetType().Name);
                parentNode.AppendChild(node);

                // Delegate serialization of the actual content object (view model, etc.)
                if (item is ILayoutSerializable ser)
                {
                    ser.Serialize(doc, node);
                }
            }
        }
    }

    /// <inheritdoc/>
    public void Deserialize(LayoutManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
        // 1) Read our attributes back

        if (node.Attributes is not null)
        {
            // Safely grab each attribute’s Value (or throw if it’s missing)
            var dockText = node.Attributes["Dock"]?.Value
                ?? throw new InvalidOperationException("Missing ‘Dock’ attribute");
            var stateText = node.Attributes["State"]?.Value
                ?? throw new InvalidOperationException("Missing ‘State’ attribute");
            var widthText = node.Attributes["Width"]?.Value
                ?? throw new InvalidOperationException("Missing ‘Width’ attribute");
            var heightText = node.Attributes["Height"]?.Value
                ?? throw new InvalidOperationException("Missing ‘Height’ attribute");
            var ptWinText = node.Attributes["ptFloatingWindow"]?.Value
                ?? throw new InvalidOperationException("Missing ‘ptFloatingWindow’ attribute");
            var sizeWinText = node.Attributes["sizeFloatingWindow"]?.Value
                ?? throw new InvalidOperationException("Missing ‘sizeFloatingWindow’ attribute");

            Dock = (Dock)Enum.Parse(typeof(Dock), dockText);
            State = (LayoutItemState)Enum.Parse(typeof(LayoutItemState), stateText);

            PaneWidth = double.Parse(widthText);
            PaneHeight = double.Parse(heightText);

            var pointConverter = TypeDescriptor.GetConverter(typeof(Point));
            _ptFloatingWindow = (Point)pointConverter.ConvertFromInvariantString(ptWinText)!;

            var sizeConverter = TypeDescriptor.GetConverter(typeof(Size));
            _sizeFloatingWindow = (Size)sizeConverter.ConvertFromInvariantString(sizeWinText)!;
        }

        // 2) Rehydrate each tab in order
        foreach (XmlNode child in node.ChildNodes)
        {
            // Ask the user-provided callback for the actual content instance
            var content = getObjectHandler(child.Name);
            Add(content);

            // If that content also implements ILayoutSerializable, let it restore itself
            if (content is ILayoutSerializable ser)
            {
                ser.Deserialize(managerToAttach, child, getObjectHandler);
            }
        }

        // 3) Finally attach ourselves back into the manager if needed
        managerToAttach?.Add(this);
    }

    #endregion

    #region Commands

    private void DockingWindowCommandExecute(ExecutedRoutedEventArgs _)
    {
        var window = CreateDockableHost(SelectedItem);
        window.Show();
    }

    private void DockingWindowCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = State != LayoutItemState.Window;
    }

    private void DockCommandExecute(ExecutedRoutedEventArgs _)
    {
        State = LayoutItemState.Docking;
    }

    private void DockCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = State != LayoutItemState.Docking;
    }

    private void TabbedDocumentCommandExecute(ExecutedRoutedEventArgs _)
    {
        TabbedDocument(SelectedItem);
    }

    private void TabbedDocumentCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = State != LayoutItemState.Document;
    }

    private void AutoHideCommandExecute(ExecutedRoutedEventArgs _)
    {
        State = State == LayoutItemState.AutoHide ? LayoutItemState.Docking : LayoutItemState.AutoHide;
    }

    private void AutoHideCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void HideCommandExecute(ExecutedRoutedEventArgs _)
    {
        Close(SelectedItem);
    }

    private void HideCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = State != LayoutItemState.Hidden;
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles mouse down event on control header
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <remarks>Save current mouse position in ptStartDrag and capture mouse event on PaneHeader object.</remarks>
    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_header is not null && !_header.IsMouseCaptured)
        {
            _headerDragStart = e.GetPosition(this);
            _header.CaptureMouse();
        }
    }

    /// <summary>
    /// Handles mouse up event on control header
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <remarks>Release any mouse capture</remarks>
    private void Header_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _header?.ReleaseMouseCapture();
    }

    /// <summary>
    /// Handles mouse move event and eventually starts dragging this control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Header_MouseMove(object sender, MouseEventArgs e)
    {
        if (_header is not null && DockManager is not null && _header.IsMouseCaptured && Math.Abs(_headerDragStart.X - e.GetPosition(this).X) > 4)
        {
            _header.ReleaseMouseCapture();
            DragContent(DockManager.PointToScreen(e.GetPosition(DockManager)), e.GetPosition(this));
        }
    }

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
            if (ItemContainerGenerator.ContainerFromItem(item) is LayoutDockItem tab)
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

    private void Tab_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is LayoutDockItem tab)
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
                CreateDockableHost(item, screenPos, offset);
                _draggedTab = null;
                _originalIndex = null;
                _lastSwapTargetIndex = null;
                return;
            }
        }

        var hoveredTab = element.VisualUpwardSearch<LayoutDockItem>();
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
                _draggedTab = ItemContainerGenerator.ContainerFromIndex(targetIndex) as LayoutDockItem;
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
