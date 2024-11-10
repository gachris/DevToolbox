using System.Windows;
using System.Windows.Controls;
using System.Xml;
using DevToolbox.Wpf.Serialization;

namespace DevToolbox.Wpf.Controls;

public class DocumentList : EditableSelector, ILayoutSerializable
{
    #region Fields/Consts

    private DockManager? _dockManager;
    private DocumentPanel? _documentPanel;

    #endregion

    #region Properties

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

    protected override DependencyObject GetContainerForItemOverride() => new DocumentControl();

    #endregion

    #region Methods

    internal void AttacheDockManager(DockManager dockManager) => _dockManager = dockManager;

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

    internal void MoveTo(DockableControl control, DocumentControl relativeControl, Dock relativeDock) => control.State = State.Document;//_documentPanel?.Remove(control);//_documentPanel?.Add(control, relativeControl, relativeDock);

    public void Serialize(XmlDocument doc, XmlNode parentNode)
    {
    }

    public void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
    }

    #endregion
}