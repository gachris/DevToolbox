using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Utils;

namespace DevToolbox.Wpf.Documents;

/// <summary>
/// Adorner used for creating visual feedback when dragging a connector in the diagram.
/// It manages the creation of temporary connection paths and handles mouse interactions
/// while establishing a connection between two connectors.
/// </summary>
internal class ConnectorAdorner : Adorner
{
    #region Fields/Consts

    /// <summary>
    /// Stores the geometry used to draw the path between connectors.
    /// </summary>
    private PathGeometry? _pathGeometry;

    /// <summary>
    /// The canvas where the connector is being dragged.
    /// </summary>
    private readonly Canvas _designerCanvas;

    /// <summary>
    /// The source connector from which the connection is being drawn.
    /// </summary>
    private readonly Connector _sourceConnector;

    /// <summary>
    /// The pen used to draw the connection line between connectors.
    /// </summary>
    private readonly Pen _drawingPen;

    /// <summary>
    /// The designer item (DiagramLayer) that is currently being hovered over by the mouse.
    /// </summary>
    private DiagramLayer? _hitDiagramLayer;

    /// <summary>
    /// The connector currently being hovered over by the mouse.
    /// </summary>
    private Connector? _hitConnector;

    /// <summary>
    /// The parent diagram canvas view.
    /// </summary>
    private DiagramCanvas? _designerView;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the designer item currently under the cursor when dragging a connector.
    /// </summary>
    private DiagramLayer? HitDiagramLayer
    {
        get => _hitDiagramLayer;
        set
        {
            if (_hitDiagramLayer != value)
            {
                if (_hitDiagramLayer != null)
                    _hitDiagramLayer.IsDragConnectionOver = false;

                _hitDiagramLayer = value;

                if (_hitDiagramLayer != null)
                    _hitDiagramLayer.IsDragConnectionOver = true;
            }
        }
    }

    /// <summary>
    /// Gets or sets the connector that is currently under the cursor.
    /// </summary>
    private Connector? HitConnector
    {
        get => _hitConnector;
        set
        {
            if (_hitConnector != value)
                _hitConnector = value;
        }
    }

    /// <summary>
    /// Gets the designer canvas view where the connector operation is happening.
    /// </summary>
    private DiagramCanvas? DiagramCanvas => _designerView ??= ((ItemsPresenter)_designerCanvas.TemplatedParent).TemplatedParent as DiagramCanvas;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorAdorner"/> class.
    /// Sets up the drawing pen and cursor for dragging connections between connectors.
    /// </summary>
    /// <param name="designer">The canvas where the connection is being drawn.</param>
    /// <param name="sourceConnector">The connector where the connection starts.</param>
    public ConnectorAdorner(Canvas designer, Connector sourceConnector) : base(designer)
    {
        _designerCanvas = designer;
        _sourceConnector = sourceConnector;
        _drawingPen = new(Brushes.LightSlateGray, 1)
        {
            LineJoin = PenLineJoin.Round
        };
        Cursor = Cursors.Cross;
    }

    #region Methods Override

    /// <summary>
    /// Handles the MouseUp event.
    /// Finalizes the connection if the user releases the mouse over another connector,
    /// creating a new connection between the source and target connectors.
    /// </summary>
    /// <param name="e">Mouse button event arguments.</param>
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (HitConnector != null && DiagramCanvas != null)
        {
            var sourceConnector = _sourceConnector;
            var sinkConnector = HitConnector;

            var newConnectionItem = DiagramCanvas.AddNewItem();

            if (newConnectionItem != null && DiagramCanvas.ContainerFromItem(newConnectionItem) is DiagramLayer newConnection)
            {
                newConnection.Attach(sourceConnector, sinkConnector);
                Panel.SetZIndex(newConnection, _designerCanvas.Children.Count);
            }
        }
        if (HitDiagramLayer != null)
        {
            HitDiagramLayer.IsDragConnectionOver = false;
        }

        if (IsMouseCaptured) ReleaseMouseCapture();

        var adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
        adornerLayer?.Remove(this);
    }

    /// <summary>
    /// Handles the MouseMove event.
    /// Tracks the mouse position and updates the visual path as the connection is being dragged.
    /// </summary>
    /// <param name="e">Mouse event arguments.</param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (!IsMouseCaptured) CaptureMouse();
            HitTesting(e.GetPosition(this));
            _pathGeometry = GetPathGeometry(e.GetPosition(this));
            InvalidateVisual();
        }
        else
        {
            if (IsMouseCaptured)
                ReleaseMouseCapture();
        }
    }

    /// <summary>
    /// Renders the connection line while dragging the connector.
    /// </summary>
    /// <param name="dc">Drawing context for rendering.</param>
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        dc.DrawGeometry(null, _drawingPen, _pathGeometry);

        // without a background the OnMouseMove event would not be fired
        // Alternative: implement a Canvas as a child of this adorner, like
        // the ConnectionAdorner does.
        dc.DrawRectangle(Brushes.Transparent, null, new(RenderSize));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the path geometry for the connection line based on the mouse position.
    /// </summary>
    /// <param name="position">The current mouse position.</param>
    /// <returns>The path geometry of the connection line.</returns>
    private PathGeometry GetPathGeometry(Point position)
    {
        PathGeometry geometry = new();

        var targetOrientation = HitConnector != null ? HitConnector.Orientation : ConnectorOrientation.None;
        var pathPoints = PathFinder.GetConnectionLine(_sourceConnector.GetInfo(), position, targetOrientation);

        if (pathPoints.Count > 0)
        {
            var figure = new PathFigure
            {
                StartPoint = pathPoints[0]
            };
            pathPoints.Remove(pathPoints[0]);
            figure.Segments.Add(new PolyLineSegment(pathPoints, true));
            geometry.Figures.Add(figure);
        }

        return geometry;
    }

    /// <summary>
    /// Performs hit testing to check whether the mouse is over a connector or a diagram item.
    /// Updates the <see cref="HitConnector"/> and <see cref="HitDiagramLayer"/> properties.
    /// </summary>
    /// <param name="hitPoint">The current mouse position for hit testing.</param>
    private void HitTesting(Point hitPoint)
    {
        var hitConnectorFlag = false;

        var hitObject = _designerCanvas.InputHitTest(hitPoint) as DependencyObject;
        while (hitObject != null &&
               hitObject != _sourceConnector.ParentDesignerItem &&
               hitObject.GetType() != typeof(Canvas))
        {
            if (hitObject is Connector)
            {
                HitConnector = hitObject as Connector;
                hitConnectorFlag = true;
            }

            if (hitObject is DiagramLayer)
            {
                HitDiagramLayer = hitObject as DiagramLayer;
                if (!hitConnectorFlag)
                    HitConnector = null;
                return;
            }
            hitObject = VisualTreeHelper.GetParent(hitObject);
        }

        HitConnector = null;
        HitDiagramLayer = null;
    }

    #endregion
}
