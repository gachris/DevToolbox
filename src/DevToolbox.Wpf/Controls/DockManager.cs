using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Serialization;
using DevToolbox.Wpf.Services;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Controls;

[TemplatePart(Name = LeftPanelTemplatePartName, Type = typeof(DockingButtonGroupControl))]
[TemplatePart(Name = RightPanelTemplatePartName, Type = typeof(DockingButtonGroupControl))]
[TemplatePart(Name = TopPanelTemplatePartName, Type = typeof(DockingButtonGroupControl))]
[TemplatePart(Name = BottomPanelTemplatePartName, Type = typeof(DockingButtonGroupControl))]
[TemplatePart(Name = DockableOverlayControlTemplatePartName, Type = typeof(DockableOverlayControl))]
[TemplatePart(Name = DockingPanelTemplatePartName, Type = typeof(DockingGroupPanel))]
public class DockManager : EditableItemsControl, IDropSurface
{
    #region Fields/Consts

    private bool _dragStarted;

    protected const string LeftPanelTemplatePartName = "PART_LeftPanel";
    protected const string RightPanelTemplatePartName = "PART_RightPanel";
    protected const string TopPanelTemplatePartName = "PART_TopPanel";
    protected const string BottomPanelTemplatePartName = "PART_BottomPanel";
    protected const string DockableOverlayControlTemplatePartName = "PART_DockableOverlayControl";
    protected const string DockingPanelTemplatePartName = "PART_DockingPanel";

    private readonly DockingDragServices _dragServices = new();
    private readonly OverlayWindow _overlayWindow;

    private Window? _owner;
    private DockableOverlayControl? _overlayControl;
    private DockingGroupPanel? _dockingPanel;
    private DockingButtonGroupControl? _topDockingButtonGroupControl;
    private DockingButtonGroupControl? _leftDockingButtonGroupControl;
    private DockingButtonGroupControl? _rightDockingButtonGroupControl;
    private DockingButtonGroupControl? _bottomDockingButtonGroupControl;

    public static readonly DependencyProperty DocumentListProperty =
        DependencyProperty.Register("DocumentList", typeof(DocumentList), typeof(DockManager), new FrameworkPropertyMetadata(null, (d, e) => (d as DockManager)?.OnDocumentListChanged(e)));

    public static readonly DependencyProperty OverlayButtonStyleProperty =
        DependencyProperty.Register("OverlayButtonStyle", typeof(Style), typeof(DockManager), new FrameworkPropertyMetadata(default));

    public static readonly DependencyProperty DockingButtonGroupControlStyleProperty =
        DependencyProperty.Register("DockingButtonGroupControlStyle", typeof(Style), typeof(DockManager), new FrameworkPropertyMetadata(default));

    #endregion

    #region Properties

    /// <summary>
    /// Gets current docking item
    /// </summary>
    public DockingButton? FocusedDockingButton { get; private set; }

    /// <summary>
    /// Gets current overlay window
    /// </summary>
    internal OverlayWindow OverlayWindow => _overlayWindow;

    /// <summary>
    /// Gets a rectangle where this surface is active
    /// </summary>
    public Rect SurfaceRectangle => new(PointToScreen(new(0, 0)), new Size(ActualWidth, ActualHeight));

    /// <summary>
    /// Gets the drag services
    /// </summary>
    internal DockingDragServices DragServices => _dragServices;

    /// <summary>
    /// Gets the owner window
    /// </summary>
    internal Window? Owner => _owner;

    public DocumentList DocumentList
    {
        get => (DocumentList)GetValue(DocumentListProperty);
        set => SetValue(DocumentListProperty, value);
    }

    public Style OverlayButtonStyle
    {
        get => (Style)GetValue(OverlayButtonStyleProperty);
        set => SetValue(OverlayButtonStyleProperty, value);
    }

    public Style DockingButtonGroupControlStyle
    {
        get => (Style)GetValue(DockingButtonGroupControlStyleProperty);
        set => SetValue(DockingButtonGroupControlStyleProperty, value);
    }

    #endregion

    static DockManager()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DockManager), new FrameworkPropertyMetadata(typeof(DockManager)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockManager"/> class.
    /// </summary>
    public DockManager()
    {
        DragServices.Register(this);

        _overlayWindow = new OverlayWindow(this);
        _overlayWindow.Hide();

        AddHandler(PreviewMouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
        AddHandler(PreviewMouseMoveEvent, new MouseEventHandler(OnMouseMove));

        CommandBindings.Add(new(DockingButton.ShowHideOverlayCommand, ShowOverlayCommandExecute, ShowOverlayCommandCanExecute));
    }

    #region Methods Override

    /// <summary>
    /// Apply default template
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _leftDockingButtonGroupControl = Template.FindName(LeftPanelTemplatePartName, this) as DockingButtonGroupControl;
        _rightDockingButtonGroupControl = Template.FindName(RightPanelTemplatePartName, this) as DockingButtonGroupControl;
        _topDockingButtonGroupControl = Template.FindName(TopPanelTemplatePartName, this) as DockingButtonGroupControl;
        _bottomDockingButtonGroupControl = Template.FindName(BottomPanelTemplatePartName, this) as DockingButtonGroupControl;

        _dockingPanel?.RemoveHandler(MouseDownEvent, new MouseButtonEventHandler(OnHideAutoHidePane));

        _dockingPanel = Template.FindName(DockingPanelTemplatePartName, this) as DockingGroupPanel;

        _dockingPanel?.AddHandler(MouseDownEvent, new MouseButtonEventHandler(OnHideAutoHidePane), true);
        _dockingPanel?.AttachDocumentList(DocumentList);

        _overlayControl = Template.FindName(DockableOverlayControlTemplatePartName, this) as DockableOverlayControl;

        if (_owner != null)
            _owner.Closed -= WindowClosed;

        _owner = Window.GetWindow(this);

        if (_owner != null)
            _owner.Closed += WindowClosed;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is DockableControl dockableControl)
        {
            dockableControl.StateChanged += OnStateChanged;
            dockableControl.DockChanged += OnDockChanged;

            if (dockableControl.State == State.Docking)
                DragServices.Register(dockableControl);

            _dockingPanel?.Add(dockableControl, dockableControl.Dock);
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is DockableControl dockableControl)
        {
            dockableControl.StateChanged -= OnStateChanged;
            dockableControl.DockChanged -= OnDockChanged;

            DragServices.Unregister(dockableControl);

            _dockingPanel?.Remove(dockableControl);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    private void OnDockChanged(object? sender, DockChangedEventArgs e)
    {
        if (sender is not DockableControl dockable) return;

        _dockingPanel?.Remove(dockable);
        _dockingPanel?.Add(dockable, dockable.Dock);
    }

    /// <summary>
    /// Creates or identifies the element that is used to display the given item.
    /// </summary>
    /// <returns>The window that is used to display the given item.</returns>
    protected override DependencyObject GetContainerForItemOverride() => new DockableControl();

    #endregion

    #region WindowContainer

    /// <summary>
    /// Creates or identifies the window that is used to display the given docking item.
    /// </summary>
    /// <returns>The window that is used to display the given docking item.</returns>
    protected internal virtual DockManagerWindow GetContainerForDockingOverride() => new DockableWindow();

    /// <summary>
    /// Creates or identifies the window that is used to display the given document item.
    /// </summary>
    /// <returns>The window that is used to display the given document item.</returns>
    protected internal virtual DockManagerWindow GetContainerForDocumentItemOverride() => new DocumentWindow();

    #endregion

    #region Commands

    private void ShowOverlayCommandExecute(object sender, ExecutedRoutedEventArgs e) => OnShowAutoHidePane((DockingButton)e.OriginalSource);

    private void ShowOverlayCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = sender is DockManager && e.OriginalSource is DockingButton;

    #endregion

    #region Arrange

    internal void MoveTo(DockableControl control, DockableControl relativeControl, Dock relativeDock)
    {
        control.Dock = relativeControl.Dock;

        control.State = State.Docking;

        _dockingPanel?.Remove(control);
        _dockingPanel?.Add(control, relativeControl, relativeDock);
    }

    internal void MoveTo(DockableControl control, DocumentControl relativeControl, Dock relativeDock) => DocumentList.MoveTo(control, relativeControl, relativeDock);

    internal void MoveTo(DocumentControl control, DocumentControl relativeControl, Dock relativeDock) => DocumentList.MoveTo(control, relativeControl, relativeDock);

    /// <summary>
    /// Called from a control when it's dropped into an other control
    /// </summary>
    /// <param name="source">Source control which is going to be closed</param>
    /// <param name="destination">Destination control which is about to host contents from SourcePane</param>
    public void MoveInto(DockableControl source, DockableControl destination)
    {
        var owner = this;

        while (source.Items.Count > 0)
        {
            var item = source.Items[0];
            source.Remove(item);
            destination.Add(item);
        }

        object? objectToRemove;
        if (owner.ItemsSource is not null)
        {
            owner.UpdateLayout();
            var index = owner.ItemContainerGenerator.IndexFromContainer(source);
            objectToRemove = owner.Items[index];
        }
        else objectToRemove = source;

        owner.Remove(objectToRemove);

        DragServices.Unregister(source);
    }

    public void MoveInto(DockableControl source, DocumentControl destination)
    {
        while (source.Items.Count > 0)
        {
            var item = source.Items[0];

            source.Remove(item);
            destination.Add(item);

            var element = destination.ContainerFromItem(item);
            if (element is DocumentItem documentItem)
            {
                documentItem.IsDockable = true;
            }
        }

        var isReadOnly = ((IList)Items).IsReadOnly;

        if (isReadOnly)
        {
            var item = this.ItemFromContainer(source);
            Remove(item);
        }
        else
        {
            Remove(source);
        }

        DragServices.Unregister(source);
    }

    public void MoveInto(DocumentControl source, DocumentControl destination)
    {
        var owner = DocumentList;

        while (source.Items.Count > 0)
        {
            var item = source.Items[0];
            source.Remove(item);
            destination.Add(item);
        }

        object? objectToRemove;
        if (owner.ItemsSource is not null)
        {
            owner.UpdateLayout();
            var index = owner.ItemContainerGenerator.IndexFromContainer(source);
            objectToRemove = owner.Items[index];
        }
        else objectToRemove = source;

        owner.Remove(objectToRemove);

        DragServices.Unregister(source);
    }

    #endregion

    #region WindowEvents

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
        _overlayWindow.Close();
    }

    #endregion

    #region Docking Buttons

    /// <summary>
    /// Add a group of docking buttons for a control docked to a dockingmanager border
    /// </summary>
    /// <param name="dockableControl"></param>
    private void AddDockingButtons(DockableControl dockableControl)
    {
        DockingButtonGroupItem dockingButtonGroupItem = new() { Dock = dockableControl.Dock };

        foreach (var item in dockableControl.Items)
        {
            int itemIdex = dockableControl.Items.IndexOf(item);
            dockableControl.UpdateLayout();

            if (item is not DockableItem containerItem)
                containerItem = (DockableItem)dockableControl.ItemContainerGenerator.ContainerFromItem(item);

            if (containerItem == null)
                throw new ArgumentNullException(nameof(containerItem));

            var dockingButtonItem = new DockingButton()
            {
                Icon = containerItem.Icon,
                Content = containerItem.Header,
                AssociatedItemIdex = itemIdex,
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
    /// Handle AutoHide/Hide commande issued by user on temporary control
    /// </summary>
    /// <param name="dockableControl"></param>
    private void RemoveDockingButtons(DockableControl dockableControl)
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

    #endregion

    #region Overlay Dockable Control

    /// <summary>
    /// Event handler which show a temporary control with a single content attached to a docking button
    /// </summary>
    /// <param name="dockingButton"></param>
    private void OnShowAutoHidePane(DockingButton dockingButton)
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
    /// Handles control state changed event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">An <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
    private void OnStateChanged(object? sender, StateChangedEventArgs e)
    {
        if (sender is not DockableControl content)
        {
            return;
        }

        if (e.NewValue == State.AutoHide)
        {
            AddDockingButtons(content);

            if (FocusedDockingButton is not null)
            {
                _overlayControl?.Show(FocusedDockingButton);
                _overlayControl?.Hide(FocusedDockingButton);
            }

            FocusedDockingButton = null;
        }

        if (e.OldValue == State.AutoHide)
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

    #endregion

    #region DragDrop Operations

    /// <summary>
    /// Begins dragging operations
    /// </summary>
    /// <param name="window">Floating window containing control which is dragged by user</param>
    /// <param name="point">Current mouse position</param>
    /// <param name="offset">Offset to be use to set floating window screen position</param>
    /// <returns>Retruns True is drag is completed, false otherwise</returns>
    public bool Drag(DockManagerWindow window, Point point, Point offset)
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
    /// Handles mousemove event
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
    /// <remarks>Releases eventually camptured mouse events</remarks>
    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (IsMouseCaptured)
        {
            _dragStarted = false;
            OverlayWindow.Owner = null;
            DragServices.EndDrag(PointToScreen(e.GetPosition(this)));
            ReleaseMouseCapture();
        }
    }

    #endregion

    #region IDropSurface

    /// <summary>
    /// Handles this sourface mouse entering (show current overlay window)
    /// </summary>
    /// <param name="point">Current mouse position</param>
    public void OnDragEnter(Point point)
    {
        OverlayWindow.Owner = DragServices.Window;
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
        //OverlayWindow.OnDragEnter(this, point);
    }

    /// <summary>
    /// Handles mouse leave event during drag (hide overlay window)
    /// </summary>
    /// <param name="point"></param>
    public void OnDragLeave(Point point)
    {
        OverlayWindow.Owner = null;
        OverlayWindow.Hide();
    }

    /// <summary>
    /// Handler drop events
    /// </summary>
    /// <param name="point">Current mouse position</param>
    /// <returns>Returns alwasy false because this surface doesn't support direct drop</returns>
    public bool OnDrop(Point point) => false;

    #endregion

    #region ISerialize

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
        //XmlDocument doc = new();
        //doc.LoadXml(xml);

        //_dockingPanel.Deserialize(this, doc.ChildNodes[0], getContentHandler);

        //List<DockableControl> addedControls = new();
        //foreach (DockableControl content in Items)
        //{
        //    if (!addedControls.Contains(content))
        //    {
        //        if (content.State == State.AutoHide)
        //        {
        //            addedControls.Add(content);
        //            AddDockingButtons(content);
        //        }
        //    }
        //}

        //FocusedDockingButton = null;
    }

    #endregion

    private void OnDocumentListChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not DocumentList documentList) return;

        documentList.AttacheDockManager(this);
        _dockingPanel?.AttachDocumentList(documentList);
    }
}