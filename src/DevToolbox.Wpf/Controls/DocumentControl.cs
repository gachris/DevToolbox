using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Serialization;

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class DocumentControl : TabControlEdit, IDropSurface, ILayoutSerializable
{
    #region Fields/Consts

    protected internal static readonly DependencyPropertyKey StatePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(State), typeof(State), typeof(DocumentControl), new FrameworkPropertyMetadata(State.Document, OnStateChanged));

    public static readonly DependencyProperty PaneWidthProperty = 
        DependencyProperty.Register(nameof(PaneWidth), typeof(double), typeof(DocumentControl), new PropertyMetadata(250D));

    public static readonly DependencyProperty PaneHeightProperty = 
        DependencyProperty.Register(nameof(PaneHeight), typeof(double), typeof(DocumentControl), new PropertyMetadata(250D));

    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register(nameof(Dock), typeof(Dock), typeof(DocumentControl), new FrameworkPropertyMetadata(Dock.Right, OnDockChanged));

    public static readonly DependencyProperty StateProperty = StatePropertyKey.DependencyProperty;

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

    public DockManager? DockManager { get; protected internal set; }

    /// <inheritdoc/>
    public Rect SurfaceRectangle => IsHidden ? new Rect() : new Rect(PointToScreen(new Point(0, 0)), new Size(ActualWidth, ActualHeight));

    public bool IsHidden => State != State.Document;

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

    #endregion

    /// <inheritdoc/>
    static DocumentControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DocumentControl), new FrameworkPropertyMetadata(typeof(DocumentControl)));
    }

    #region Methods Override

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (DockManager == null)
            DockManager = this.FindVisualAncestor<DockManager>();
        if (DockManager == null && TemplatedParent != null)
            DockManager = TemplatedParent.FindVisualAncestor<DockManager>();
        if (DockManager == null && VisualParent != null)
            DockManager = VisualParent.FindVisualAncestor<DockManager>();

        if (DockManager == null)
            throw new InvalidOperationException($"{nameof(TemplatedParent)} or {nameof(VisualParent)} of {typeof(DockableControl)} must be {typeof(DockManager)} type");
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride() => new DocumentItem();

    ///// <inheritdoc />
    //public override void DragOverEx(IDropInfo dropInfo)
    //{
    //    base.DragOverEx(dropInfo);

    //    if (!CanAcceptData(dropInfo))
    //    {
    //        var oldContainer = dropInfo.DragInfo.VisualSourceItem;
    //        var startDragPoint = dropInfo.DragEventArgs.GetPosition(DockManager);
    //        var offset = dropInfo.DragEventArgs.GetPosition(oldContainer);

    //        DragContent(dropInfo.DragInfo.VisualSourceItem, dropInfo.DragInfo.SourceItem, startDragPoint, offset);
    //    }
    //}

    #endregion

    #region Methods

    private static void OnDockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dockableControl = (DocumentControl)d;
        dockableControl.OnDockChanged((Dock)e.OldValue, (Dock)e.NewValue);
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dockableControl = (DocumentControl)d;
        dockableControl.OnStateChanged((State)e.OldValue, (State)e.NewValue);
    }

    private void OnDockChanged(Dock oldValue, Dock newValue)
    {
        DockChanged?.Invoke(this, new DockChangedEventArgs(oldValue, newValue));
    }

    private void OnStateChanged(State oldValue, State newValue)
    {
        SaveSize();
        StateChanged?.Invoke(this, new StateChangedEventArgs(oldValue, newValue));
    }

    /// <summary>
    /// Dock this control to a destination control border
    /// </summary>
    /// <param name="control"></param>
    /// <param name="relativeDock"></param>
    public virtual void MoveTo(DocumentControl control, Dock relativeDock) => DockManager?.MoveTo(this, control, relativeDock);

    /// <summary>
    /// Move contained contents into a destination control and close this one
    /// </summary>
    /// <param name="control"></param>
    public void MoveInto(DocumentControl control) => DockManager?.MoveInto(this, control);

    /// <summary>
    /// Move contained contents into a destination control and close this one
    /// </summary>
    public void MoveInto()
    {
        if (DockManager is null || State == State.Document) return;

        var item = DockManager.DocumentList.ItemFromContainer(this);

        var isReadOnlyDockManager = ((IList)DockManager.DocumentList.Items).IsReadOnly;
        if (!isReadOnlyDockManager)
            DockManager.DocumentList.Remove(this);
        else
        {
            var currentItem = DockManager.DocumentList.ItemFromContainer(this);
            DockManager.DocumentList.Remove(currentItem);
        }

        var isReadOnly = ((IList)Items).IsReadOnly;

        DocumentControl? newElement;
        if (isReadOnly)
        {
            var newItem = DockManager.DocumentList.Add();
            newElement = DockManager.DocumentList.ContainerFromItem(newItem) as DocumentControl;
        }
        else newElement = DockManager.DocumentList.Add() as DocumentControl;

        if (newElement is null) return;

        newElement.Add(item);
        newElement.State = State.Document;
        newElement.DockManager = DockManager;
    }

    /// <summary>
    /// 
    /// </summary>
    //public override void Remove(object value)
    //{
    //    base.Remove(value);

    //    //if (Items.Count == 0)
    //    //    DockManager.DragServices.Unregister(this);
    //}

    public virtual void SaveSize()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    /// <param name="item"></param>
    /// <param name="startDragPoint"></param>
    /// <param name="offset"></param>
    private void DragContent(DependencyObject element, object item, Point startDragPoint, Point offset)
    {
        if (element is DockableItem || element is DocumentItem documentItem && documentItem.IsDockable)
            CreateDockableHost(element, item, startDragPoint, offset);
        else
            CreateDocumentHost(element, item, startDragPoint, offset);
    }

    private void CreateDockableHost(DependencyObject _, object item, Point startDragPoint, Point offset)
    {
        if (DockManager is null || State == State.Window) return;

        var window = DockManager.GetContainerForDockingOverride();

        Remove(item);

        if (Items.Count == 0)
        {
            var isReadOnlyDockManager = ((IList)DockManager.DocumentList.Items).IsReadOnly;
            if (!isReadOnlyDockManager)
                DockManager.DocumentList.Remove(this);
            else
            {
                var currentItem = DockManager.DocumentList.ItemFromContainer(this);
                DockManager.DocumentList.Remove(currentItem);
            }
        }

        var isReadOnly = ((IList)Items).IsReadOnly;

        DockableControl? newElement;
        if (isReadOnly)
        {
            var newItem = DockManager.Add();
            newElement = DockManager.ContainerFromItem(newItem) as DockableControl;
        }
        else newElement = DockManager.Add() as DockableControl;

        if (newElement is null) return;

        newElement.Add(item);
        newElement.DockManager = DockManager;
        newElement.State = State.Window;

        window.Content = newElement;
        DockManager.Drag(window, startDragPoint, offset);
    }

    private void CreateDocumentHost(DependencyObject _, object item, Point startDragPoint, Point offset)
    {
        if (DockManager is null || State == State.Window) return;
        var window = DockManager.GetContainerForDocumentItemOverride();

        DocumentControl? newElement = null;

        if (Items.Count == 1)
            newElement = this;
        else
            Remove(item);

        var isReadOnly = ((IList)Items).IsReadOnly;

        if (newElement is null)
        {
            if (isReadOnly)
            {
                var newItem = DockManager.DocumentList.Add();
                newElement = DockManager.DocumentList.ContainerFromItem(newItem) as DocumentControl;
            }
            else newElement = DockManager.DocumentList.Add() as DocumentControl;

            if (newElement is null) return;

            newElement.Add(item);
        }

        newElement.State = State.Window;
        newElement.DockManager = DockManager;

        window.Content = newElement;

        DockManager.Drag(window, startDragPoint, offset);
    }

    /// <inheritdoc/>
    public virtual void OnDragEnter(Point point) => DockManager?.OverlayWindow.OnDragEnter(this, point);

    /// <inheritdoc/>
    public virtual void OnDragOver(Point point) => DockManager?.OverlayWindow.OnDragOver(this, point);

    /// <inheritdoc/>
    public virtual void OnDragLeave(Point point) => DockManager?.OverlayWindow.OnDragLeave(this, point);

    /// <inheritdoc/>
    public virtual bool OnDrop(Point point) => false;

    /// <inheritdoc/>
    public virtual void Serialize(XmlDocument doc, XmlNode parentNode)
    {
        //parentNode.Attributes.Append(doc.CreateAttribute("Width"));
        //parentNode.Attributes["Width"].Value = PaneWidth.ToString();
        //parentNode.Attributes.Append(doc.CreateAttribute("Height"));
        //parentNode.Attributes["Height"].Value = PaneHeight.ToString();

        //foreach (object content in Items)
        //{
        //    if (content is DockableItem dockableContent)
        //    {
        //        XmlNode nodeDockableContent = doc.CreateElement(dockableContent.GetType().ToString());
        //        parentNode.AppendChild(nodeDockableContent);
        //    }
        //}
    }

    /// <inheritdoc/>
    public virtual void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
        //DockManager = managerToAttach;

        //PaneWidth = double.Parse(node.Attributes["Width"].Value);
        //PaneHeight = double.Parse(node.Attributes["Height"].Value);

        //foreach (XmlNode childNode in node.ChildNodes)
        //    Add(getObjectHandler(childNode.Name));

        //DockManager.DragServices.Register(this);
    }

    #endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member