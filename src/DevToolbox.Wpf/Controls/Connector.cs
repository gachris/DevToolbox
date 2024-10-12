using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DevToolbox.Wpf.Documents;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a connector control that is used within a diagram to connect different items.
/// The connector allows for connections to be created by dragging and dropping, and tracks its position.
/// </summary>
public class Connector : Control
{
    #region Fields/Consts

    /// <summary>
    /// Stores the initial drag point when a drag operation begins.
    /// </summary>
    private Point? _dragStartPoint;

    /// <summary>
    /// The designer item (DiagramLayer) that this connector belongs to.
    /// </summary>
    private DiagramLayer? _parentDesignerItem;

    /// <summary>
    /// Occurs when the position of the connector changes.
    /// </summary>
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;

    /// <summary>
    /// Dependency property for the connector's position.
    /// </summary>
    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register(nameof(Position), typeof(Point), typeof(Connector), new PropertyMetadata(default(Point), OnPositionChanged));

    #endregion

    #region Properties

    /// <summary>
    /// Gets the designer item (DiagramLayer) that this connector belongs to.
    /// </summary>
    public DiagramLayer? ParentDesignerItem => _parentDesignerItem ??= DataContext as DiagramLayer;

    /// <summary>
    /// Gets or sets the orientation of the connector (e.g., Top, Bottom, Left, Right).
    /// </summary>
    public ConnectorOrientation Orientation { get; set; }

    /// <summary>
    /// A list of connections that are linked to this connector.
    /// </summary>
    public List<DiagramLayer> Connections { get; private set; }

    /// <summary>
    /// Gets or sets the position of the connector within its parent canvas.
    /// </summary>
    public Point Position
    {
        get => (Point)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    #endregion

    static Connector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Connector), new FrameworkPropertyMetadata(typeof(Connector)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Connector"/> class.
    /// </summary>
    public Connector()
    {
        Connections = [];
        LayoutUpdated += new EventHandler(OnLayoutUpdated);
    }

    #region Methods Override

    /// <summary>
    /// Handles the MouseLeftButtonDown event.
    /// Starts a drag operation for connecting this connector.
    /// </summary>
    /// <param name="e">Mouse button event arguments.</param>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        var canvas = GetDesignerCanvas(this);
        if (canvas != null)
        {
            _dragStartPoint = new Point?(e.GetPosition(canvas));
            e.Handled = true;
        }
    }

    /// <summary>
    /// Handles the MouseMove event.
    /// Initiates the drag adorner for the connector if a drag operation is in progress.
    /// </summary>
    /// <param name="e">Mouse event arguments.</param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        // if mouse button is not pressed we have no drag operation, ...
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            _dragStartPoint = null;
        }

        // but if mouse button is pressed and start point value is set we do have one
        if (!_dragStartPoint.HasValue)
        {
            return;
        }

        // create connection adorner 
        var canvas = GetDesignerCanvas(this);
        if (canvas is null)
        {
            return;
        }

        var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
        if (adornerLayer != null)
        {
            var adorner = new ConnectorAdorner(canvas, this);
            adornerLayer.Add(adorner);

            e.Handled = true;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Called when the Position property changes. Raises the PositionChanged event.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    /// <param name="e">Dependency property changed event arguments.</param>
    private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var connector = (Connector)d;
        connector.OnPositionChanged((Point)e.OldValue, (Point)e.NewValue);
    }

    /// <summary>
    /// Triggers the PositionChanged event when the position of the connector changes.
    /// </summary>
    /// <param name="oldValue">The old position value.</param>
    /// <param name="newValue">The new position value.</param>
    private void OnPositionChanged(Point oldValue, Point newValue)
    {
        PositionChanged?.Invoke(this, new PositionChangedEventArgs(oldValue, newValue));
    }

    /// <summary>
    /// Retrieves information about the connector, including its position and size.
    /// </summary>
    /// <returns>A <see cref="ConnectorInfo"/> object containing the connector's details.</returns>
    internal ConnectorInfo GetInfo()
    {
        return new ConnectorInfo
        {
            Left = Canvas.GetLeft(ParentDesignerItem),
            Top = Canvas.GetTop(ParentDesignerItem),
            Size = new Size(ParentDesignerItem?.ActualWidth ?? 0.0, ParentDesignerItem?.ActualHeight ?? 0.0),
            Orientation = Orientation,
            Position = Position
        };
    }

    /// <summary>
    /// Retrieves the parent <see cref="Canvas"/> (DesignerCanvas) of the connector by traversing the visual tree.
    /// </summary>
    /// <param name="element">The starting element for the search.</param>
    /// <returns>The parent <see cref="Canvas"/> if found, otherwise null.</returns>
    private static Canvas? GetDesignerCanvas(DependencyObject element)
    {
        while (element is not null and not Canvas)
        {
            element = VisualTreeHelper.GetParent(element);
        }

        return element as Canvas;
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Updates the position of the connector when the layout is updated.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        var designer = GetDesignerCanvas(this);
        if (designer is null)
        {
            return;
        }

        Position = TransformToAncestor(designer).Transform(new Point(Width / 2, Height / 2));
    }

    #endregion
}