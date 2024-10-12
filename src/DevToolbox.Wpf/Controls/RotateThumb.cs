using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a draggable control that allows for rotating a <see cref="DesignLayer"/> 
/// element within a <see cref="Canvas"/>.
/// </summary>
public class RotateThumb : Thumb
{
    private Canvas? _canvas; // The parent canvas for the rotation
    private Point _centerPoint; // The center point around which the element will rotate
    private Vector _startVector; // The vector from the center point to the initial mouse position
    private RotateTransform? _rotateTransform; // The rotate transform applied to the designer item
    private double _initialAngle; // The initial angle of rotation

    /// <summary>
    /// Initializes a new instance of the <see cref="RotateThumb"/> class.
    /// Sets up event handlers for drag operations.
    /// </summary>
    public RotateThumb()
    {
        DragStarted += OnRotateDragStarted;
        DragDelta += OnRotateDragDelta;
    }

    /// <summary>
    /// Event handler for the <see cref="Thumb.DragStarted"/> event.
    /// Initializes rotation parameters when the drag operation starts.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnRotateDragStarted(object sender, DragStartedEventArgs e)
    {
        if (DataContext is DesignLayer designerItem)
        {
            _canvas = (Canvas)VisualTreeHelper.GetParent(designerItem);

            if (_canvas != null)
            {
                // Calculate the center point based on the RenderTransformOrigin
                _centerPoint = designerItem.TranslatePoint(
                    new Point(designerItem.DesiredSize.Width * designerItem.RenderTransformOrigin.X,
                              designerItem.DesiredSize.Height * designerItem.RenderTransformOrigin.Y),
                              _canvas);

                Point startPoint = Mouse.GetPosition(_canvas);
                _startVector = Point.Subtract(startPoint, _centerPoint);

                _rotateTransform = designerItem.RenderTransform as RotateTransform;
                if (_rotateTransform == null)
                {
                    // Initialize with a new RotateTransform if none exists
                    designerItem.RenderTransform = new RotateTransform(0);
                    _initialAngle = 0;
                }
                else
                {
                    _initialAngle = _rotateTransform.Angle;
                }
            }
        }
    }

    /// <summary>
    /// Event handler for the <see cref="Thumb.DragDelta"/> event.
    /// Updates the angle of rotation during the drag operation.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnRotateDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is DesignLayer designerItem && _canvas != null)
        {
            var currentPoint = Mouse.GetPosition(_canvas);
            var deltaVector = Point.Subtract(currentPoint, _centerPoint);

            // Calculate the angle of rotation based on the mouse movement
            var angle = Vector.AngleBetween(_startVector, deltaVector);

            if (designerItem.RenderTransform is RotateTransform rotateTransform)
                rotateTransform.Angle = _initialAngle + Math.Round(angle, 0);

            // Invalidate the measure to update the layout
            designerItem.InvalidateMeasure();
        }
    }
}
