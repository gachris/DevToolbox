using System;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using DevToolbox.Wpf.Documents;
using DevToolbox.Wpf.Utils;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a layer in a diagram that handles connectors, connections, and visual elements.
/// This class derives from <see cref="DesignLayer"/> and provides additional functionality for
/// managing connectors and adorners related to diagram connections.
/// </summary>
[TemplatePart(Name = "PART_ResizeDecorator", Type = typeof(Control))]
[TemplatePart(Name = "PART_ConnectorDecorator", Type = typeof(Control))]
public class DiagramLayer : DesignLayer
{
    #region Fields/Consts

    /// <summary>
    /// Holds the source connector for the diagram layer.
    /// </summary>
    private Connector? _source;

    /// <summary>
    /// Holds the sink connector for the diagram layer.
    /// </summary>
    private Connector? _sink;

    /// <summary>
    /// Stores the connection adorner used to represent a visual cue for connections.
    /// </summary>
    private Adorner? _connectionAdorner;

    /// <summary>
    /// Read-only dependency property key for determining if this is a connection.
    /// </summary>
    private static readonly DependencyPropertyKey IsConnectionPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsConnection), typeof(bool), typeof(DiagramLayer), new PropertyMetadata(false));

    /// <summary>
    /// Dependency property indicating if a connection is being dragged over this diagram layer.
    /// </summary>
    public static readonly DependencyProperty IsDragConnectionOverProperty =
        DependencyProperty.Register(nameof(IsDragConnectionOver), typeof(bool), typeof(DiagramLayer), new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Dependency property to hold the template for the connector decorator.
    /// </summary>
    public static readonly DependencyProperty ConnectorDecoratorTemplateProperty =
        DependencyProperty.Register(nameof(ConnectorDecoratorTemplate), typeof(ControlTemplate), typeof(DiagramLayer));

    /// <summary>
    /// Dependency property to store the anchor position of the source connector.
    /// </summary>
    public static readonly DependencyProperty AnchorPositionSourceProperty =
        DependencyProperty.Register(nameof(AnchorPositionSource), typeof(Point), typeof(DiagramLayer), new PropertyMetadata(default(Point)));

    /// <summary>
    /// Dependency property to store the anchor position of the sink connector.
    /// </summary>
    public static readonly DependencyProperty AnchorPositionSinkProperty =
        DependencyProperty.Register(nameof(AnchorPositionSink), typeof(Point), typeof(DiagramLayer), new PropertyMetadata(default(Point)));

    /// <summary>
    /// Dependency property representing the arrow symbol for the source connector.
    /// </summary>
    public static readonly DependencyProperty SourceArrowSymbolProperty =
        DependencyProperty.Register(nameof(SourceArrowSymbol), typeof(ArrowSymbol), typeof(DiagramLayer), new PropertyMetadata(ArrowSymbol.None));

    /// <summary>
    /// Dependency property representing the arrow symbol for the sink connector.
    /// </summary>
    public static readonly DependencyProperty SinkArrowSymbolProperty =
        DependencyProperty.Register(nameof(SinkArrowSymbol), typeof(ArrowSymbol), typeof(DiagramLayer), new PropertyMetadata(ArrowSymbol.Arrow));

    /// <summary>
    /// Dependency property to hold the position of the label for the connection.
    /// </summary>
    public static readonly DependencyProperty LabelPositionProperty =
        DependencyProperty.Register(nameof(LabelPosition), typeof(Point), typeof(DiagramLayer), new PropertyMetadata(default(Point)));

    /// <summary>
    /// Dependency property for storing the angle of the sink anchor point.
    /// </summary>
    public static readonly DependencyProperty AnchorAngleSinkProperty =
        DependencyProperty.Register(nameof(AnchorAngleSink), typeof(double), typeof(DiagramLayer), new PropertyMetadata(default(double)));

    /// <summary>
    /// Dependency property for storing the angle of the source anchor point.
    /// </summary>
    public static readonly DependencyProperty AnchorAngleSourceProperty =
        DependencyProperty.Register(nameof(AnchorAngleSource), typeof(double), typeof(DiagramLayer), new PropertyMetadata(default(double)));

    /// <summary>
    /// Dependency property for storing the path geometry of the connection line.
    /// </summary>
    public static readonly DependencyProperty PathGeometryProperty =
        DependencyProperty.Register(nameof(PathGeometry), typeof(PathGeometry), typeof(DiagramLayer), new FrameworkPropertyMetadata(default, OnPathGeometryChanged));

    /// <summary>
    /// Dependency property to hold the dash pattern for the connection stroke.
    /// </summary>
    public static readonly DependencyProperty StrokeDashArrayProperty =
        DependencyProperty.Register(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(DiagramLayer), new PropertyMetadata(default));

    /// <summary>
    /// Dependency property for determining if this is a connection.
    /// </summary>
    public static readonly DependencyProperty IsConnectionProperty = IsConnectionPropertyKey.DependencyProperty;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the source connector.
    /// </summary>
    public Connector? Source
    {
        get => _source;
        set
        {
            if (_source != value)
            {
                if (_source != null)
                {
                    _source.PositionChanged -= OnConnectorPositionChanged;
                    _source.Connections.Remove(this);
                }

                _source = value;

                if (_source != null)
                {
                    _source.Connections.Add(this);
                    _source.PositionChanged += OnConnectorPositionChanged;
                }

                UpdatePathGeometry();
            }
        }
    }

    /// <summary>
    /// Gets or sets the sink connector.
    /// </summary>
    public Connector? Sink
    {
        get => _sink;
        set
        {
            if (_sink != value)
            {
                if (_sink != null)
                {
                    _sink.PositionChanged -= OnConnectorPositionChanged;
                    _sink.Connections.Remove(this);
                }

                _sink = value;

                if (_sink != null)
                {
                    _sink.Connections.Add(this);
                    _sink.PositionChanged += OnConnectorPositionChanged;
                }

                UpdatePathGeometry();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this layer is a connection.
    /// </summary>
    public bool IsConnection
    {
        get => (bool)GetValue(IsConnectionProperty);
        private set => SetValue(IsConnectionPropertyKey, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the layer is currently being dragged over by a connection.
    /// </summary>
    public bool IsDragConnectionOver
    {
        get => (bool)GetValue(IsDragConnectionOverProperty);
        set => SetValue(IsDragConnectionOverProperty, value);
    }

    /// <summary>
    /// Gets or sets the control template for the connector decorator.
    /// </summary>
    public ControlTemplate ConnectorDecoratorTemplate
    {
        get => (ControlTemplate)GetValue(ConnectorDecoratorTemplateProperty);
        set => SetValue(ConnectorDecoratorTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the anchor position for the source connector.
    /// </summary>
    public Point AnchorPositionSource
    {
        get => (Point)GetValue(AnchorPositionSourceProperty);
        set => SetValue(AnchorPositionSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the angle of the source connector's anchor.
    /// </summary>
    public double AnchorAngleSource
    {
        get => (double)GetValue(AnchorAngleSourceProperty);
        set => SetValue(AnchorAngleSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the anchor position for the sink connector.
    /// </summary>
    public Point AnchorPositionSink
    {
        get => (Point)GetValue(AnchorPositionSinkProperty);
        set => SetValue(AnchorPositionSinkProperty, value);
    }

    /// <summary>
    /// Gets or sets the angle of the sink connector's anchor.
    /// </summary>
    public double AnchorAngleSink
    {
        get => (double)GetValue(AnchorAngleSinkProperty);
        set => SetValue(AnchorAngleSinkProperty, value);
    }

    /// <summary>
    /// Gets or sets the arrow symbol for the source.
    /// </summary>
    public ArrowSymbol SourceArrowSymbol
    {
        get => (ArrowSymbol)GetValue(SourceArrowSymbolProperty);
        set => SetValue(SourceArrowSymbolProperty, value);
    }

    /// <summary>
    /// Gets or sets the arrow symbol for the sink.
    /// </summary>
    public ArrowSymbol SinkArrowSymbol
    {
        get => (ArrowSymbol)GetValue(SinkArrowSymbolProperty);
        set => SetValue(SinkArrowSymbolProperty, value);
    }

    /// <summary>
    /// Gets or sets the position for the label.
    /// </summary>
    public Point LabelPosition
    {
        get => (Point)GetValue(LabelPositionProperty);
        set => SetValue(LabelPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets the path geometry for the connection.
    /// </summary>
    public PathGeometry PathGeometry
    {
        get => (PathGeometry)GetValue(PathGeometryProperty);
        set => SetValue(PathGeometryProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke dash array for the connection path.
    /// </summary>
    public DoubleCollection? StrokeDashArray
    {
        get => (DoubleCollection)GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    #endregion

    static DiagramLayer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramLayer), new FrameworkPropertyMetadata(typeof(DiagramLayer)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramLayer"/> class.
    /// </summary>
    public DiagramLayer() : base()
    {
    }

    #region Methods Override

    /// <summary>
    /// Handles mouse down events to manage selection behavior for the connection.
    /// </summary>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        e.Handled = false;

        if (!IsConnection)
        {
            return;
        }

        if (VisualTreeHelper.GetParent(this) is not Canvas)
        {
            return;
        }

        if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
        {
            if (IsSelected)
            {
                InternalParent?.Unselect(this);
            }
            else
            {
                InternalParent?.Select(this);
            }
        }
        else if (!IsSelected)
        {
            InternalParent?.UnselectAll();
            InternalParent?.Select(this);
        }

        Focus();
    }

    /// <summary>
    /// Called when the selected state of the connection changes.
    /// </summary>
    protected override void OnIsSelectedChanged(bool isSeleted)
    {
        base.OnIsSelectedChanged(isSeleted);

        if (!IsConnection)
        {
            return;
        }

        if (isSeleted)
        {
            ShowAdorner();
        }
        else
        {
            HideAdorner();
        }
    }

    /// <summary>
    /// Serializes the connection to an XML element.
    /// </summary>
    public override XElement Serialize()
    {
        return IsConnection
            ? new XElement(
                "Connection",
                new XElement("SourceID", Source?.ParentDesignerItem?.ID),
                new XElement("SinkID", Sink?.ParentDesignerItem?.ID),
                new XElement("SourceConnectorName", Source?.Name),
                new XElement("SinkConnectorName", Sink?.Name),
                new XElement("SourceArrowSymbol", SourceArrowSymbol),
                new XElement("SinkArrowSymbol", SinkArrowSymbol),
                new XElement("zIndex", Canvas.GetZIndex(this)))
            : base.Serialize();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Called when the path geometry property changes.
    /// </summary>
    private static void OnPathGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var diagramLayer = (DiagramLayer)d;
        diagramLayer.OnPathGeometryChanged((PathGeometry)e.OldValue, (PathGeometry)e.NewValue);
    }

    /// <summary>
    /// Handles the path geometry change and updates anchor positions.
    /// </summary>
    private void OnPathGeometryChanged(PathGeometry oldValue, PathGeometry newValue)
    {
        UpdateAnchorPosition();
    }

    /// <summary>
    /// Attaches the connection to the specified source and sink connectors.
    /// </summary>
    internal void Attach(Connector sourceConnector, Connector sinkConnector)
    {
        IsConnection = true;
        Source = sourceConnector;
        Sink = sinkConnector;
        Unloaded += new RoutedEventHandler(Connection_Unloaded);
    }

    /// <summary>
    /// Updates the path geometry for the connection based on the source and sink connectors.
    /// </summary>
    private void UpdatePathGeometry()
    {
        if (Source == null || Sink == null)
        {
            return;
        }

        var geometry = new PathGeometry();
        var linePoints = PathFinder.GetConnectionLine(Source.GetInfo(), Sink.GetInfo(), true);

        if (linePoints.Count <= 0)
        {
            return;
        }

        var figure = new PathFigure
        {
            StartPoint = linePoints[0]
        };

        linePoints.Remove(linePoints[0]);
        figure.Segments.Add(new PolyLineSegment(linePoints, true));
        geometry.Figures.Add(figure);

        PathGeometry = geometry;
    }

    /// <summary>
    /// Updates the anchor positions and angles based on the path geometry.
    /// </summary>
    private void UpdateAnchorPosition()
    {
        PathGeometry.GetPointAtFractionLength(0, out Point pathStartPoint, out Point pathTangentAtStartPoint);
        PathGeometry.GetPointAtFractionLength(1, out Point pathEndPoint, out Point pathTangentAtEndPoint);
        PathGeometry.GetPointAtFractionLength(0.5, out Point pathMidPoint, out Point pathTangentAtMidPoint);

        AnchorAngleSource = Math.Atan2(-pathTangentAtStartPoint.Y, -pathTangentAtStartPoint.X) * (180 / Math.PI);
        AnchorAngleSink = Math.Atan2(pathTangentAtEndPoint.Y, pathTangentAtEndPoint.X) * (180 / Math.PI);

        pathStartPoint.Offset(-pathTangentAtStartPoint.X * 5, -pathTangentAtStartPoint.Y * 5);
        pathEndPoint.Offset(pathTangentAtEndPoint.X * 5, pathTangentAtEndPoint.Y * 5);

        AnchorPositionSource = pathStartPoint;
        AnchorPositionSink = pathEndPoint;
        LabelPosition = pathMidPoint;
    }

    /// <summary>
    /// Displays the adorner for the connection.
    /// </summary>
    private void ShowAdorner()
    {
        if (_connectionAdorner != null)
        {
            _connectionAdorner.Visibility = Visibility.Visible;

            return;
        }

        AttachAdorner();
    }

    /// <summary>
    /// Hides the adorner for the connection.
    /// </summary>
    private void HideAdorner()
    {
        if (_connectionAdorner != null)
        {
            _connectionAdorner.Visibility = Visibility.Collapsed;
        }
    }

    private void AttachAdorner()
    {
        if (_connectionAdorner is not null || VisualTreeHelper.GetParent(this) is not Canvas designer)
        {
            return;
        }

        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer != null)
        {
            _connectionAdorner = new ConnectionAdorner(designer, this);
            adornerLayer.Add(_connectionAdorner);
        }
    }

    private void DetachAdorner()
    {
        if (_connectionAdorner is null)
        {
            return;
        }

        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer != null)
        {
            adornerLayer.Remove(_connectionAdorner);
            _connectionAdorner = null;
        }
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles the Unloaded event to clean up resources.
    /// </summary>
    private void Connection_Unloaded(object sender, RoutedEventArgs e)
    {
        Source = null;
        Sink = null;

        DetachAdorner();
    }

    /// <summary>
    /// Handles changes in the connector position to update the path geometry.
    /// </summary>
    private void OnConnectorPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        UpdatePathGeometry();
    }

    #endregion
}