using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Utils;

namespace DevToolbox.Wpf.Documents;

/// <summary>
/// Represents an adorner that visually connects two connectors with a line.
/// This adorner allows for dragging connection points to create dynamic links between items.
/// </summary>
internal class ConnectionAdorner : Adorner
{
    #region Fields/Consts

    private PathGeometry? _pathGeometry;
    private Connector? _fixConnector;
    private Connector? _dragConnector;
    private Thumb? _sourceDragThumb;
    private Thumb? _sinkDragThumb;
    private Connector? _hitConnector;
    private DiagramLayer? _hitDiagramLayer;
    private readonly Pen _drawingPen;
    private readonly Canvas _designerCanvas;
    private readonly Canvas _adornerCanvas;
    private readonly DiagramLayer _connection;
    private readonly VisualCollection _visualChildren;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the hit diagram layer and updates its drag connection state.
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
    /// Gets or sets the connector currently under the mouse cursor.
    /// </summary>
    private Connector? HitConnector
    {
        get => _hitConnector;
        set => _hitConnector = value;
    }

    /// <summary>
    /// Gets the number of visual children in the adorner.
    /// </summary>
    protected override int VisualChildrenCount => _visualChildren.Count;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionAdorner"/> class.
    /// </summary>
    /// <param name="designer">The canvas where the diagram is drawn.</param>
    /// <param name="connection">The diagram layer representing the connection.</param>
    public ConnectionAdorner(Canvas designer, DiagramLayer connection) : base(designer)
    {
        _designerCanvas = designer;
        _adornerCanvas = new();
        _visualChildren = new(this)
        {
            _adornerCanvas
        };

        _connection = connection;

        var anchorPositionSourcePropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DiagramLayer.AnchorPositionSourceProperty, typeof(DiagramLayer));
        anchorPositionSourcePropertyDescriptor.AddValueChanged(_connection, OnAnchorPositionSourceChanged);

        var anchorPositionSinkPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DiagramLayer.AnchorPositionSinkProperty, typeof(DiagramLayer));
        anchorPositionSinkPropertyDescriptor.AddValueChanged(_connection, OnAnchorPositionSinkChanged);

        InitializeDragThumbs();

        _drawingPen = new(Brushes.LightSlateGray, 1)
        {
            LineJoin = PenLineJoin.Round
        };

        Unloaded += ConnectionAdorner_Unloaded;
    }

    #region Methods Override

    /// <summary>
    /// Gets the visual child at the specified index.
    /// </summary>
    /// <param name="index">The index of the child.</param>
    /// <returns>The visual child.</returns>
    protected override Visual GetVisualChild(int index)
    {
        return _visualChildren[index];
    }

    /// <summary>
    /// Renders the adorner with the connection line.
    /// </summary>
    /// <param name="dc">The drawing context to render to.</param>
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        dc.DrawGeometry(null, _drawingPen, _pathGeometry);
    }

    /// <summary>
    /// Arranges the adorner's visual children.
    /// </summary>
    /// <param name="finalSize">The final size of the adorner.</param>
    /// <returns>The size that the adorner arranged itself to.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        _adornerCanvas.Arrange(new(0, 0, _designerCanvas.ActualWidth, _designerCanvas.ActualHeight));
        return finalSize;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initializes the drag thumbs for the source and sink connectors.
    /// </summary>
    private void InitializeDragThumbs()
    {
        var dragThumbStyle = _connection.FindResource("ConnectionAdornerThumbStyle") as Style;

        //source drag thumb
        _sourceDragThumb = new();
        Canvas.SetLeft(_sourceDragThumb, _connection.AnchorPositionSource.X);
        Canvas.SetTop(_sourceDragThumb, _connection.AnchorPositionSource.Y);
        _adornerCanvas.Children.Add(_sourceDragThumb);
        if (dragThumbStyle != null)
        {
            _sourceDragThumb.Style = dragThumbStyle;
        }

        _sourceDragThumb.DragDelta += ThumbDragThumb_DragDelta;
        _sourceDragThumb.DragStarted += ThumbDragThumb_DragStarted;
        _sourceDragThumb.DragCompleted += ThumbDragThumb_DragCompleted;

        // sink drag thumb
        _sinkDragThumb = new();
        Canvas.SetLeft(_sinkDragThumb, _connection.AnchorPositionSink.X);
        Canvas.SetTop(_sinkDragThumb, _connection.AnchorPositionSink.Y);
        _adornerCanvas.Children.Add(_sinkDragThumb);
        if (dragThumbStyle != null)
        {
            _sinkDragThumb.Style = dragThumbStyle;
        }

        _sinkDragThumb.DragDelta += ThumbDragThumb_DragDelta;
        _sinkDragThumb.DragStarted += ThumbDragThumb_DragStarted;
        _sinkDragThumb.DragCompleted += ThumbDragThumb_DragCompleted;
    }

    /// <summary>
    /// Updates the path geometry based on the current mouse position.
    /// </summary>
    /// <param name="position">The current mouse position.</param>
    /// <returns>The updated path geometry.</returns>
    private PathGeometry UpdatePathGeometry(Point position)
    {
        var geometry = new PathGeometry();

        if (_fixConnector is null) return geometry;
        if (_dragConnector is null) return geometry;

        var targetOrientation = HitConnector != null ? HitConnector.Orientation : _dragConnector.Orientation;
        var linePoints = PathFinder.GetConnectionLine(_fixConnector.GetInfo(), position, targetOrientation);

        if (linePoints.Count > 0)
        {
            var figure = new PathFigure
            {
                StartPoint = linePoints[0]
            };
            linePoints.Remove(linePoints[0]);
            figure.Segments.Add(new PolyLineSegment(linePoints, true));
            geometry.Figures.Add(figure);
        }

        return geometry;
    }

    /// <summary>
    /// Performs hit testing to determine if a connector is currently under the mouse.
    /// </summary>
    /// <param name="hitPoint">The point to test for hits.</param>
    private void HitTesting(Point hitPoint)
    {
        var hitConnectorFlag = false;

        var hitObject = _designerCanvas.InputHitTest(hitPoint) as DependencyObject;
        while (hitObject != null &&
               hitObject != _fixConnector?.ParentDesignerItem &&
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

    #region Events Subscriptions

    /// <summary>
    /// Handles changes to the source anchor position.
    /// </summary>
    private void OnAnchorPositionSourceChanged(object? sender, EventArgs e)
    {
        Canvas.SetLeft(_sourceDragThumb, _connection.AnchorPositionSource.X);
        Canvas.SetTop(_sourceDragThumb, _connection.AnchorPositionSource.Y);
    }

    /// <summary>
    /// Handles changes to the sink anchor position.
    /// </summary>
    private void OnAnchorPositionSinkChanged(object? sender, EventArgs e)
    {
        Canvas.SetLeft(_sinkDragThumb, _connection.AnchorPositionSink.X);
        Canvas.SetTop(_sinkDragThumb, _connection.AnchorPositionSink.Y);
    }

    /// <summary>
    /// Handles the completion of dragging a thumb.
    /// Sets the connector based on the current hit state.
    /// </summary>
    private void ThumbDragThumb_DragCompleted(object? sender, DragCompletedEventArgs e)
    {
        if (HitConnector != null)
        {
            if (_connection != null)
            {
                if (_connection.Source == _fixConnector)
                    _connection.Sink = HitConnector;
                else
                    _connection.Source = HitConnector;
            }
        }

        HitDiagramLayer = null;
        HitConnector = null;
        _pathGeometry = null;
        if (_connection != null)
            _connection.StrokeDashArray = null;
        InvalidateVisual();
    }

    /// <summary>
    /// Handles the start of dragging a thumb.
    /// Initializes drag state and visual appearance.
    /// </summary>
    private void ThumbDragThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        HitDiagramLayer = null;
        HitConnector = null;
        _pathGeometry = null;
        Cursor = Cursors.Cross;
        _connection.StrokeDashArray = new(new double[] { 1, 2 });

        if (sender == _sourceDragThumb)
        {
            _fixConnector = _connection.Sink;
            _dragConnector = _connection.Source;
        }
        else if (sender == _sinkDragThumb)
        {
            _dragConnector = _connection.Sink;
            _fixConnector = _connection.Source;
        }
    }

    /// <summary>
    /// Handles the dragging of a thumb, updating the path geometry.
    /// </summary>
    private void ThumbDragThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var currentPosition = Mouse.GetPosition(this);
        HitTesting(currentPosition);
        _pathGeometry = UpdatePathGeometry(currentPosition);
        InvalidateVisual();
    }

    /// <summary>
    /// Cleans up event handlers when the adorner is unloaded.
    /// </summary>
    private void ConnectionAdorner_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_sourceDragThumb != null)
        {
            _sourceDragThumb.DragDelta -= ThumbDragThumb_DragDelta;
            _sourceDragThumb.DragStarted -= ThumbDragThumb_DragStarted;
            _sourceDragThumb.DragCompleted -= ThumbDragThumb_DragCompleted;
        }

        if (_sinkDragThumb != null)
        {
            _sinkDragThumb.DragDelta -= ThumbDragThumb_DragDelta;
            _sinkDragThumb.DragStarted -= ThumbDragThumb_DragStarted;
            _sinkDragThumb.DragCompleted -= ThumbDragThumb_DragCompleted;
        }
    }

    #endregion
}