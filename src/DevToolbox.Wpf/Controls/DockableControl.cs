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

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A <see cref="TabControlEdit"/> that can be docked, floated, auto-hidden or hidden in a <see cref="DockManager"/>.
/// </summary>
[TemplatePart(Name = PART_Header, Type = typeof(ContentControl))]
public sealed class DockableControl : TabControlEdit, IDropSurface, ILayoutSerializable
{
    #region Fields/Consts

    private const string PART_Header = nameof(PART_Header);

    private DockableItem? _draggedTab;
    private int? _originalIndex;
    private Point _headerDragStart;
    private ContentControl? _header;
    private Point _ptFloatingWindow = new(0, 0);
    private Size _sizeFloatingWindow = new(300, 300);

    /// <summary>
    /// Occurs when the <see cref="Dock"/> property has changed.
    /// </summary>
    public event EventHandler<DockChangedEventArgs>? DockChanged;

    /// <summary>
    /// Occurs when the <see cref="State"/> property has changed.
    /// </summary>
    public event EventHandler<StateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Identifies the <see cref="ShowSeparator"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowSeparatorProperty =
        DependencyProperty.Register(nameof(ShowSeparator), typeof(bool), typeof(DockableControl), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Identifies the <see cref="SeparatorBrush"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SeparatorBrushProperty =
        DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush), typeof(DockableControl), new FrameworkPropertyMetadata(BackgroundProperty.DefaultMetadata.DefaultValue));

    /// <summary>
    /// Identifies the <see cref="HeaderTemplate"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(DockableControl), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Identifies the <see cref="Header"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(DockableControl), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Identifies the <see cref="Icon"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(DockableControl), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Identifies the <see cref="PaneWidth"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PaneWidthProperty =
        DependencyProperty.Register(nameof(PaneWidth), typeof(double), typeof(DockableControl), new PropertyMetadata(250D));

    /// <summary>
    /// Identifies the <see cref="PaneHeight"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PaneHeightProperty =
        DependencyProperty.Register(nameof(PaneHeight), typeof(double), typeof(DockableControl), new PropertyMetadata(250D));

    /// <summary>
    /// Identifies the <see cref="Dock"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register(nameof(Dock), typeof(Dock), typeof(DockableControl), new FrameworkPropertyMetadata(Dock.Right, OnDockChanged));

    /// <summary>
    /// Identifies the <see cref="State"/> dependency property key.
    /// </summary>
    private static readonly DependencyPropertyKey StatePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(State), typeof(State), typeof(DockableControl), new FrameworkPropertyMetadata(State.Docking, OnStateChanged));

    /// <summary>
    /// Identifies the <see cref="ShowHeader"/> dependency property key.
    /// </summary>
    private static readonly DependencyPropertyKey ShowHeaderPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ShowHeader), typeof(bool), typeof(DockableControl), new FrameworkPropertyMetadata(true));

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
        new(nameof(DockableWindowCommand), nameof(DockableWindowCommand), typeof(DockableControl));

    /// <summary>
    /// Command to dock the currently floating window back into the layout.
    /// </summary>
    public static RoutedUICommand DockCommand { get; } =
        new(nameof(Dock), nameof(Dock), typeof(DockableControl));

    /// <summary>
    /// Command to convert the selected pane into a tabbed document in the main document area.
    /// </summary>
    public static RoutedUICommand TabbedDocumentCommand { get; } =
        new(nameof(TabbedDocumentCommand), nameof(TabbedDocumentCommand), typeof(DockableControl));

    /// <summary>
    /// Command to toggle the auto-hide state of the pane (slide it in and out on hover).
    /// </summary>
    public static RoutedUICommand AutoHideCommand { get; } =
        new(nameof(AutoHideCommand), nameof(AutoHideCommand), typeof(DockableControl));

    /// <summary>
    /// Command to hide the pane completely until reactivated.
    /// </summary>
    public static RoutedUICommand HideCommand { get; } =
        new(nameof(HideCommand), nameof(HideCommand), typeof(DockableControl));

    /// <summary>
    /// Gets the <see cref="DockManager"/> that contains this control.
    /// </summary>
    internal DockManager? DockManager { get; set; }

    /// <inheritdoc/>
    public Rect SurfaceRectangle => IsHidden ? new Rect() : new Rect(PointToScreen(new Point(0, 0)), new Size(ActualWidth, ActualHeight));

    /// <summary>
    /// Return true if control is hidden, ie State is different from PaneState.Docked
    /// </summary>
    public bool IsHidden => State != State.Docking;

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
    /// Gets or sets whether a separator line is shown between header and content.
    /// </summary>
    public bool ShowSeparator
    {
        get => (bool)GetValue(ShowSeparatorProperty);
        set => SetValue(ShowSeparatorProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to draw the separator line.
    /// </summary>
    public Brush SeparatorBrush
    {
        get => (Brush)GetValue(SeparatorBrushProperty);
        set => SetValue(SeparatorBrushProperty, value);
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
    public State State
    {
        get => (State)GetValue(StateProperty);
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

    static DockableControl()
    {
        var typeFromHandle = typeof(DockableControl);

        DefaultStyleKeyProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(typeFromHandle));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
            new(DockableWindowCommand, (sender, e) => (sender as DockableControl)?.DockingWindowCommandExecute(e), (sender, e) => (sender as DockableControl)?.DockingWindowCommandCanExecute(e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
             new(DockCommand, (sender, e) => (sender as DockableControl)?.DockCommandExecute(e), (sender, e) => (sender as DockableControl)?.DockCommandCanExecute(e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
            new(TabbedDocumentCommand, (sender, e) => (sender as DockableControl)?.TabbedDocumentCommandExecute(e), (sender, e) => (sender as DockableControl)?.TabbedDocumentCommandCanExecute(e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
            new(AutoHideCommand, (sender, e) => (sender as DockableControl)?.AutoHideCommandExecute(e), (sender, e) => (sender as DockableControl)?.AutoHideCommandCanExecute(e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle,
            new(HideCommand, (sender, e) => (sender as DockableControl)?.HideCommandExecute(e), (sender, e) => (sender as DockableControl)?.HideCommandCanExecute(e)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockableControl"/> class,
    /// and hooks into item generator to wire up tab header drag events.
    /// </summary>
    /// <summary>
    /// Initializes a new instance of the <see cref="DockableControl"/> class,
    /// and registers for the <see cref="ItemContainerGenerator.StatusChanged"/> event
    /// to hook up drag handlers on each tab header when containers are generated.
    /// </summary>
    public DockableControl()
    {
        ItemContainerGenerator.StatusChanged += OnItemGeneratorStatusChanged;
    }

    #region Methods Override

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DockManager ??= this.FindVisualAncestor<DockManager>();

        if (DockManager == null && TemplatedParent != null)
            DockManager = TemplatedParent.FindVisualAncestor<DockManager>();

        if (DockManager == null && VisualParent != null)
            DockManager = VisualParent.FindVisualAncestor<DockManager>();

        if (DockManager == null)
            throw new InvalidOperationException($"{nameof(TemplatedParent)} or {nameof(VisualParent)} of {typeof(DockableControl)} must be {typeof(DockManager)} type");

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

        if (State == State.Docking)
        {
            DockManager.DragServices.Register(this);
        }
    }

    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DockableItem();
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
        var dockableControl = (DockableControl)d;
        dockableControl.OnDockChanged((Dock)e.OldValue, (Dock)e.NewValue);
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dockableControl = (DockableControl)d;
        dockableControl.OnStateChanged((State)e.OldValue, (State)e.NewValue);
    }

    private void OnDockChanged(Dock oldValue, Dock newValue)
    {
        DockChanged?.Invoke(this, new DockChangedEventArgs(oldValue, newValue));
    }

    private void OnStateChanged(State oldValue, State newValue)
    {
        SaveSize();

        ShowHeader = newValue != State.Window;

        StateChanged?.Invoke(this, new StateChangedEventArgs(oldValue, newValue));
    }

    /// <inheritdoc/>
    public void MoveTo(DockableControl control, Dock relativeDock)
    {
        DockManager?.MoveTo(this, control, relativeDock);
    }

    /// <inheritdoc/>
    public void MoveTo(DocumentControl control, Dock relativeDock)
    {
        DockManager?.MoveTo(this, control, relativeDock);
    }

    /// <inheritdoc/>
    public void MoveInto(DockableControl control)
    {
        DockManager?.MoveInto(this, control);
    }

    /// <inheritdoc/>
    public void MoveInto(DocumentControl control)
    {
        DockManager?.MoveInto(this, control);
    }

    /// <inheritdoc/>
    public void SaveSize()
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

    /// <inheritdoc/>
    public void OnDragEnter(Point point)
    {
        DockManager?.OverlayWindow.OnDragEnter(this, point);
    }

    /// <inheritdoc/>
    public void OnDragOver(Point point)
    {
        DockManager?.OverlayWindow.OnDragOver(this, point);
    }

    /// <inheritdoc/>
    public void OnDragLeave(Point point)
    {
        DockManager?.OverlayWindow.OnDragLeave(this, point);
    }

    /// <inheritdoc/>
    public bool OnDrop(Point point)
    {
        return false;
    }

    /// <summary>
    /// Create and show a floating window hosting this control
    /// </summary> 
    private void FloatingWindow(object item)
    {
        if (DockManager is null || State == State.Window)
        {
            return;
        }

        DockableControl? newElement;
        var window = DockManager.GetContainerForDockingOverride();

        Remove(item);

        if (Items.Count == 0)
        {
            var isReadOnlyDockManager = ((IList)DockManager.Items).IsReadOnly;
            if (!isReadOnlyDockManager)
            {
                DockManager.Remove(this);
            }
            else
            {
                var currentItem = DockManager.ItemFromContainer(this);
                DockManager.Remove(currentItem);
            }
        }

        var isReadOnly = ((IList)Items).IsReadOnly;

        if (isReadOnly)
        {
            var newItem = DockManager.Add();
            newElement = DockManager.ContainerFromItem(newItem) as DockableControl;
        }
        else
        {
            newElement = DockManager.Add() as DockableControl;
        }

        if (newElement is null)
        {
            return;
        }

        newElement.Add(item);
        newElement.DockManager = DockManager;
        newElement.State = State.Window;

        window.Content = newElement;
        window.Owner = DockManager.Owner;
        RestoreWindowSizeAndPosition(window);
        window.Show();
    }

    /// <summary>
    /// Create and show a dockable window hosting this control
    /// </summary>
    private void Docking()
    {
        State = State.Docking;
    }

    private void TabbedDocument(object item)
    {
        if (DockManager is null)
        {
            return;
        }

        Remove(item);

        var documentControlElement = (DocumentControl)DockManager.DocumentList.ContainerFromItem(DockManager.DocumentList.Items[0]);
        documentControlElement.Add(item);

        if (Items.Count == 0)
        {
            var isReadOnlyDockManager = ((IList)DockManager.Items).IsReadOnly;
            if (isReadOnlyDockManager)
            {
                var currentItem = DockManager.ItemFromContainer(this);
                DockManager.Remove(currentItem);
            }
            else DockManager.Remove(this);
        }
    }

    /// <summary>
    /// Auto-hide this control 
    /// </summary>
    private void AutoHide()
    {
        State = State == State.AutoHide ? State.Docking : State.AutoHide;
    }

    /// <summary>
    /// Initiate a dragging operation of this control, relative DockManager is also involved
    /// </summary>
    /// <param name="startDragPoint"></param>
    /// <param name="offset"></param>
    private void DragDockableControl(Point startDragPoint, Point offset)
    {
        if (DockManager is null)
        {
            return;
        }

        var window = DockManager.GetContainerForDockingOverride();

        State = State.Window;
        window.Content = this;
        RestoreWindowSizeAndPosition(window);

        window.Show();
        DockManager.Drag(window, startDragPoint, offset);
    }

    private void DragContent(object item, Point startDragPoint, Point offset)
    {
        if (Items.Count == 1 || DockManager is null)
        {
            return;
        }

        DockableControl? newElement;
        var window = DockManager.GetContainerForDockingOverride();

        Remove(item);

        var isReadOnly = ((IList)Items).IsReadOnly;

        if (isReadOnly)
        {
            var newItem = DockManager.Add();
            newElement = DockManager.ContainerFromItem(newItem) as DockableControl;
        }
        else
        {
            newElement = DockManager.Add() as DockableControl;
        }

        if (newElement is null)
        {
            return;
        }

        newElement.Add(item);
        newElement.DockManager = DockManager;
        newElement.State = State.Window;

        window.Content = newElement;

        DockManager.Drag(window, startDragPoint, offset);
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
            if (ItemContainerGenerator.ContainerFromItem(item) is DockableItem)
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
    public void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
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
            State = (State)Enum.Parse(typeof(State), stateText);

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
        FloatingWindow(SelectedItem);
    }

    private void DockingWindowCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = State != State.Window;
    }

    private void DockCommandExecute(ExecutedRoutedEventArgs _)
    {
        Docking();
    }

    private void DockCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = State != State.Docking;
    }

    private void TabbedDocumentCommandExecute(ExecutedRoutedEventArgs _)
    {
        TabbedDocument(SelectedItem);
    }

    private void TabbedDocumentCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = State != State.Document;
    }

    private void AutoHideCommandExecute(ExecutedRoutedEventArgs _)
    {
        AutoHide();
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
        e.CanExecute = State != State.Hidden;
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
            DragDockableControl(DockManager.PointToScreen(e.GetPosition(DockManager)), e.GetPosition(this));
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
            if (ItemContainerGenerator.ContainerFromItem(item) is DockableItem tab)
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
        if (sender is DockableItem tab)
        {
            _draggedTab = tab;
            _originalIndex = Items.IndexOf(tab.DataContext ?? tab);
        }
    }

    private void Tab_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_draggedTab == null || e.LeftButton != MouseButtonState.Pressed || !_originalIndex.HasValue)
            return;

        var point = e.GetPosition(this);
        var tabPanel = _draggedTab.VisualUpwardSearch<Panel>();
        if (tabPanel != null)
        {
            var relPos = e.GetPosition(tabPanel);
            if (relPos.X < 0 || relPos.X > tabPanel.ActualWidth || relPos.Y < 0 || relPos.Y > tabPanel.ActualHeight)
            {
                var screenPos = PointToScreen(point);
                var offset = e.GetPosition(_draggedTab);
                var item = _draggedTab.DataContext ?? _draggedTab;
                DragContent(item, screenPos, offset);
                _draggedTab = null;
                _originalIndex = null;
                return;
            }
        }

        var element = InputHitTest(point) as DependencyObject;
        var hoveredTab = element.VisualUpwardSearch<DockableItem>();
        if (hoveredTab != null && hoveredTab != _draggedTab)
        {
            var sourceIndex = _originalIndex.Value;
            var targetIndex = Items.IndexOf(hoveredTab.DataContext ?? hoveredTab);
            if (sourceIndex != targetIndex && sourceIndex >= 0 && targetIndex >= 0)
            {
                var items = ((IList)Items).IsReadOnly && ItemsSource is IList src ? src : Items;
                var item = items[sourceIndex];
                items.RemoveAt(sourceIndex);
                items.Insert(targetIndex, item);
                SelectedIndex = targetIndex;
                _originalIndex = targetIndex;
                _draggedTab = ItemContainerGenerator.ContainerFromIndex(targetIndex) as DockableItem;
            }
        }
    }

    private void Tab_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _draggedTab = null;
        _originalIndex = null;
    }

    #endregion
}
