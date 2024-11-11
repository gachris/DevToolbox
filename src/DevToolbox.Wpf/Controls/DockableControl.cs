using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Serialization;

namespace DevToolbox.Wpf.Controls;

[TemplatePart(Name = PART_Header, Type = typeof(ContentControl))]
public class DockableControl : TabControlEdit, IDropSurface, ILayoutSerializable
{
    #region Fields/Consts

    private const string PART_Header = nameof(PART_Header);

    private static readonly RoutedUICommand _dockableWindowCommand = new(nameof(DockableWindowCommand), nameof(DockableWindowCommand), typeof(DockableControl));
    private static readonly RoutedUICommand _dockCommand = new(nameof(Dock), nameof(Dock), typeof(DockableControl));
    private static readonly RoutedUICommand _tabbedDocumentCommand = new(nameof(TabbedDocumentCommand), nameof(TabbedDocumentCommand), typeof(DockableControl));
    private static readonly RoutedUICommand _autoHideCommand = new(nameof(AutoHideCommand), nameof(AutoHideCommand), typeof(DockableControl));
    private static readonly RoutedUICommand _hideCommand = new(nameof(HideCommand), nameof(HideCommand), typeof(DockableControl));

    private Point _ptStartDrag;
    private ContentControl? _header;
    private Point _ptFloatingWindow = new(0, 0);
    private Size _sizeFloatingWindow = new(300, 300);

    public static readonly DependencyProperty ShowSeparatorProperty =
        DependencyProperty.Register(nameof(ShowSeparator), typeof(bool), typeof(DockableControl), new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty SeparatorBrushProperty =
        DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush), typeof(DockableControl), new FrameworkPropertyMetadata(BackgroundProperty.DefaultMetadata.DefaultValue));

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(DockableControl), new FrameworkPropertyMetadata(default));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(DockableControl), new FrameworkPropertyMetadata(default));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(DockableControl), new FrameworkPropertyMetadata(default));

    public static readonly DependencyProperty PaneWidthProperty =
        DependencyProperty.Register(nameof(PaneWidth), typeof(double), typeof(DockableControl), new PropertyMetadata(250D));

    public static readonly DependencyProperty PaneHeightProperty =
        DependencyProperty.Register(nameof(PaneHeight), typeof(double), typeof(DockableControl), new PropertyMetadata(250D));

    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register(nameof(Dock), typeof(Dock), typeof(DockableControl), new FrameworkPropertyMetadata(Dock.Right, OnDockChanged));

    private static readonly DependencyPropertyKey StatePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(State), typeof(State), typeof(DockableControl), new FrameworkPropertyMetadata(State.Docking, OnStateChanged));

    private static readonly DependencyPropertyKey ShowHeaderPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ShowHeader), typeof(bool), typeof(DockableControl), new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty StateProperty = StatePropertyKey.DependencyProperty;
    public static readonly DependencyProperty ShowHeaderProperty = ShowHeaderPropertyKey.DependencyProperty;

    /// <summary>
    /// Event raised when Dock property is changed
    /// </summary>
    public event EventHandler<DockChangedEventArgs>? DockChanged;

    /// <summary>
    /// Event raised when State property is changed
    /// </summary>
    public event EventHandler<StateChangedEventArgs>? StateChanged;

    #endregion

    #region Properties

    public static RoutedCommand DockableWindowCommand => _dockableWindowCommand;

    public static RoutedUICommand DockCommand => _dockCommand;

    public static RoutedUICommand TabbedDocumentCommand => _tabbedDocumentCommand;

    public static RoutedUICommand AutoHideCommand => _autoHideCommand;

    public static RoutedUICommand HideCommand => _hideCommand;

    public DockManager? DockManager { get; protected internal set; }

    /// <inheritdoc/>
    public Rect SurfaceRectangle => IsHidden ? new Rect() : new Rect(PointToScreen(new Point(0, 0)), new Size(ActualWidth, ActualHeight));

    /// <summary>
    /// Return true if control is hidden, ie State is different from PaneState.Docked
    /// </summary>
    public bool IsHidden => State != State.Docking;

    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public DataTemplate HeaderTemplate
    {
        get => (DataTemplate)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public bool ShowSeparator
    {
        get => (bool)GetValue(ShowSeparatorProperty);
        set => SetValue(ShowSeparatorProperty, value);
    }

    public Brush SeparatorBrush
    {
        get => (Brush)GetValue(SeparatorBrushProperty);
        set => SetValue(SeparatorBrushProperty, value);
    }

    public double PaneWidth
    {
        get => (double)GetValue(PaneWidthProperty);
        set => SetValue(PaneWidthProperty, value);
    }

    public double PaneHeight
    {
        get => (double)GetValue(PaneHeightProperty);
        set => SetValue(PaneHeightProperty, value);
    }

    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    public State State
    {
        get => (State)GetValue(StateProperty);
        set => SetValue(StatePropertyKey, value);
    }

    public bool ShowHeader
    {
        get => (bool)GetValue(ShowHeaderProperty);
        protected set => SetValue(ShowHeaderPropertyKey, value);
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

    #region Methods Override

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DockManager ??= this.FindVisualAncestor<DockManager>();

        if (DockManager == null && TemplatedParent != null)
        {
            DockManager = TemplatedParent.FindVisualAncestor<DockManager>();
        }

        if (DockManager == null && VisualParent != null)
        {
            DockManager = VisualParent.FindVisualAncestor<DockManager>();
        }

        if (DockManager == null)
        {
            throw new InvalidOperationException($"{nameof(TemplatedParent)} or {nameof(VisualParent)} of {typeof(DockableControl)} must be {typeof(DockManager)} type");
        }

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

    ///// <inheritdoc/>
    //public override void DragOverEx(IDropInfo dropInfo)
    //{
    //    base.DragOverEx(dropInfo);

    //    if (!CanAcceptData(dropInfo))
    //    {
    //        CaptureMouse();
    //        var oldContainer = dropInfo.DragInfo.VisualSourceItem;
    //        var startDragPoint = dropInfo.DragEventArgs.GetPosition(DockManager);
    //        var offset = dropInfo.DragEventArgs.GetPosition(oldContainer);
    //        ReleaseMouseCapture();
    //        DragContent(dropInfo.DragInfo.VisualSourceItem, dropInfo.DragInfo.SourceItem, startDragPoint, offset);
    //    }

    //    //if (Parent is not DockableControl parent)
    //    //    parent = (VisualParent as Panel)?.TemplatedParent as DockableControl;

    //    //if (parent == null)
    //    //    base.OnHeaderMouseMove(sender, e);
    //    //else
    //    //{
    //    //    if (_header.IsMouseCaptured && Math.Abs(_ptStartDrag.X - e.GetPosition(this).X) > 4)
    //    //    {
    //    //        _header.ReleaseMouseCapture();
    //    //        parent.DragContent(this, e);
    //    //    }
    //    //}
    //}

    /// <summary>
    /// Create and show a floating window hosting this control
    /// </summary> 
    public void DockingWindow(object item)
    {
        if (DockManager is null || State == State.Window)
        {
            return;
        }

        DockableItem? element;

        if (IsItemItsOwnContainer(item))
        {
            element = item as DockableItem;
            item = this.ItemFromContainer(item) ?? item;
        }
        else
        {
            element = this.ContainerFromItem(item) as DockableItem;
        }

        if (element is null)
        {
            return;
        }

        DockingWindow(element, item);
    }

    private void DockingWindow(DockableItem element, object item)
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
        SetWindowSizeAndPosition(window);
        window.Show();
    }

    /// <summary>
    /// Create and show a dockable window hosting this control
    /// </summary>
    public void Docking()
    {
        State = State.Docking;
    }

    /// <summary>
    /// Show contained contents as documents and close this control
    /// </summary>
    public void TabbedDocument(object item)
    {
        if (DockManager is null || State == State.Document)
        {
            return;
        }

        DockableItem? element;

        if (IsItemItsOwnContainer(item))
        {
            element = item as DockableItem;
            item = this.ItemFromContainer(item) ?? item;
        }
        else
        {
            element = this.ContainerFromItem(item) as DockableItem;
        }

        if (element is null)
        {
            return;
        }

        TabbedDocument(element, item);
    }

    private void TabbedDocument(DockableItem element, object item)
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
    public void AutoHide()
    {
        State = State == State.AutoHide ? State.Docking : State.AutoHide;
    }

    /// <summary>
    /// Hide this control 
    /// </summary>
    public void Hide(object value)
    {
        State = State.Hidden;
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
        SetWindowSizeAndPosition(window);

        window.Show();
        DockManager.Drag(window, startDragPoint, offset);
    }

    private void DragContent(DependencyObject _, object item, Point startDragPoint, Point offset)
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

    internal void SetWindowSizeAndPosition(Window window)
    {
        window.Left = _ptFloatingWindow.X;
        window.Top = _ptFloatingWindow.Y;
        window.Width = _sizeFloatingWindow.Width;
        window.Height = _sizeFloatingWindow.Height;
    }

    /// <inheritdoc/>
    public void Serialize(XmlDocument doc, XmlNode parentNode)
    {
        //SaveSize();

        //parentNode.Attributes.Append(doc.CreateAttribute("Dock"));
        //parentNode.Attributes["Dock"].Value = Dock.ToString();
        //parentNode.Attributes.Append(doc.CreateAttribute("State"));
        //parentNode.Attributes["State"].Value = State.ToString();
        //parentNode.Attributes.Append(doc.CreateAttribute("LastState"));
        ////parentNode.Attributes["LastState"].Value = _lastState.ToString();

        //parentNode.Attributes.Append(doc.CreateAttribute("ptFloatingWindow"));
        //parentNode.Attributes["ptFloatingWindow"].Value = TypeDescriptor.GetConverter(typeof(Point)).ConvertToInvariantString(_ptFloatingWindow);
        //parentNode.Attributes.Append(doc.CreateAttribute("sizeFloatingWindow"));
        //parentNode.Attributes["sizeFloatingWindow"].Value = TypeDescriptor.GetConverter(typeof(Size)).ConvertToInvariantString(_sizeFloatingWindow);

        //parentNode.Attributes.Append(doc.CreateAttribute("Width"));
        //parentNode.Attributes["Width"].Value = PaneWidth.ToString();
        //parentNode.Attributes.Append(doc.CreateAttribute("Height"));
        //parentNode.Attributes["Height"].Value = PaneHeight.ToString();

        //foreach (var content in Items)
        //{
        //    if (content is DockableItem)
        //    {
        //        XmlNode nodeDockableContent = doc.CreateElement(content.GetType().ToString());
        //        parentNode.AppendChild(nodeDockableContent);
        //    }
        //}
    }

    /// <inheritdoc/>
    public void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
        ////DockManager = managerToAttach;

        //PaneWidth = double.Parse(node.Attributes["Width"].Value);
        //PaneHeight = double.Parse(node.Attributes["Height"].Value);

        //foreach (XmlNode childNode in node.ChildNodes)
        //    Add(getObjectHandler(childNode.Name));

        //Dock = (Dock)Enum.Parse(typeof(Dock), node.Attributes["Dock"].Value);
        //State = State.Docking;
        ////base.SetValue(StatePropertyKey, (DockableState)Enum.Parse(typeof(DockableState), node.Attributes["State"].Value));
        ////_lastState = (DockableState)Enum.Parse(typeof(DockableState), node.Attributes["LastState"].Value);

        //_ptFloatingWindow = (Point)TypeDescriptor.GetConverter(typeof(Point)).ConvertFromInvariantString(node.Attributes["ptFloatingWindow"].Value);
        //_sizeFloatingWindow = (Size)TypeDescriptor.GetConverter(typeof(Size)).ConvertFromInvariantString(node.Attributes["sizeFloatingWindow"].Value);

        ////if (State == State.Window)
        ////    DockingWindow();

        //DockManager?.Add(this);
    }

    #endregion

    #region Commands

    private void DockingWindowCommandExecute(ExecutedRoutedEventArgs e)
    {
        DockingWindow(SelectedItem);
    }

    private void DockingWindowCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = State != State.Window;
    }

    private void DockCommandExecute(ExecutedRoutedEventArgs e)
    {
        Docking();
    }

    private void DockCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = State != State.Docking;
    }

    private void TabbedDocumentCommandExecute(ExecutedRoutedEventArgs e)
    {
        TabbedDocument(SelectedItem);
    }

    private void TabbedDocumentCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = State != State.Document;
    }

    private void AutoHideCommandExecute(ExecutedRoutedEventArgs e)
    {
        AutoHide();
    }

    private void AutoHideCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void HideCommandExecute(ExecutedRoutedEventArgs e)
    {
        Hide(SelectedItem);
    }

    private void HideCommandCanExecute(CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = State != State.Hidden;
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles mouse douwn event on control header
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <remarks>Save current mouse position in ptStartDrag and capture mouse event on PaneHeader object.</remarks>
    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_header is not null && !_header.IsMouseCaptured)
        {
            _ptStartDrag = e.GetPosition(this);
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
    /// Handles mouse move event and eventually starts draging this control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Header_MouseMove(object sender, MouseEventArgs e)
    {
        if (_header is not null && DockManager is not null && _header.IsMouseCaptured && Math.Abs(_ptStartDrag.X - e.GetPosition(this).X) > 4)
        {
            _header.ReleaseMouseCapture();
            DragDockableControl(DockManager.PointToScreen(e.GetPosition(DockManager)), e.GetPosition(this));
        }
    }

    #endregion
}
