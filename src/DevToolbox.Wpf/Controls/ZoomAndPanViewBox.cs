using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevToolbox.Wpf.Helpers;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A class that wraps up zooming and panning of it's content.
/// </summary>
[TemplatePart(Name = PART_Content, Type = typeof(Canvas))]
[TemplatePart(Name = PART_SizingBorder, Type = typeof(Border))]
[TemplatePart(Name = PART_DraggingBorder, Type = typeof(Border))]
public class ZoomAndPanViewBox : Control
{
    #region Fields/Consts

    /// <summary>
    /// Identifies the part of the control that contains the main content.
    /// This part is typically a <see cref="Canvas"/> or similar element that holds the zoomable content.
    /// </summary>
    protected const string PART_Content = nameof(PART_Content);

    /// <summary>
    /// Identifies the part of the control that serves as the sizing border.
    /// This border is used for visual feedback during resizing operations and may be displayed
    /// when the user is interacting with the control to change its size.
    /// </summary>
    protected const string PART_SizingBorder = nameof(PART_SizingBorder);

    /// <summary>
    /// Identifies the part of the control that acts as a dragging border.
    /// This border provides visual feedback while panning or dragging content within the control.
    /// It helps users understand the area being interacted with during zoom and pan operations.
    /// </summary>
    protected const string PART_DraggingBorder = nameof(PART_DraggingBorder);

    /// <summary>
    /// The control for creating a drag border
    /// </summary>
    private Border? _dragBorder;

    /// <summary>
    /// The control for creating a drag border
    /// </summary>
    private Border? _sizingBorder;

    /// <summary>
    /// The control for containing a zoom border
    /// </summary>
    private Canvas? _viewportCanvas;

    /// <summary>
    /// Specifies the current state of the mouse handling logic.
    /// </summary>
    private MouseHandlingMode _mouseHandlingMode = MouseHandlingMode.None;

    /// <summary>
    /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
    /// </summary>
    private Point _origContentMouseDownPoint;

    /// <summary>
    /// Identifies the <see cref="Visual"/> dependency property.
    /// This property holds a reference to the visual element that will be zoomed and panned.
    /// </summary>
    public static readonly DependencyProperty VisualProperty =
        DependencyProperty.Register(nameof(Visual), typeof(FrameworkElement), typeof(ZoomAndPanViewBox), new FrameworkPropertyMetadata(default, (d, e) => ((ZoomAndPanViewBox)d).OnVisualChanged(e)));

    /// <summary>
    /// Identifies the <see cref="OverlayBrush"/> dependency property.
    /// This property allows you to specify a brush that can be used for overlay effects,
    /// such as highlighting areas during zooming or providing visual feedback.
    /// </summary>
    public static readonly DependencyProperty OverlayBrushProperty =
        DependencyProperty.Register(nameof(OverlayBrush), typeof(Brush), typeof(ZoomAndPanViewBox), new PropertyMetadata(default));

    /// <summary>
    /// Identifies the <see cref="ZoomAndPanControl"/> dependency property.
    /// This property holds a reference to the <see cref="ZoomAndPanControl"/> instance
    /// that handles the zooming and panning operations.
    /// </summary>
    public static readonly DependencyProperty ZoomAndPanControlProperty =
        DependencyProperty.Register(nameof(ZoomAndPanControl), typeof(ZoomAndPanControl), typeof(ZoomAndPanViewBox), new PropertyMetadata(default));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the overlay brush used in the view box.
    /// </summary>
    public Brush OverlayBrush
    {
        get => (Brush)GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    /// <summary>
    /// The X coordinate of the content focus, this is the point that we are focusing on when zooming.
    /// </summary>
    public FrameworkElement Visual
    {
        get => (FrameworkElement)GetValue(VisualProperty);
        set => SetValue(VisualProperty, value);
    }

    /// <summary>
    /// The X coordinate of the content focus, this is the point that we are focusing on when zooming.
    /// </summary>
    public ZoomAndPanControl ZoomAndPanControl
    {
        get => (ZoomAndPanControl)GetValue(ZoomAndPanControlProperty);
        set => SetValue(ZoomAndPanControlProperty, value);
    }

    #endregion

    #region Static 

    /// <summary>
    /// Static constructor to define metadata for the control (and link it to the style in Generic.xaml).
    /// </summary>
    static ZoomAndPanViewBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomAndPanViewBox), new FrameworkPropertyMetadata(typeof(ZoomAndPanViewBox)));
    }

    #endregion

    #region Overrides

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _viewportCanvas = Template.FindName(PART_Content, this) as Canvas;
        _sizingBorder = Template.FindName(PART_SizingBorder, this) as Border;
        _dragBorder = Template.FindName(PART_DraggingBorder, this) as Border;
        SetBackground(Visual);
    }

    /// <inheritdoc/>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        if (ActualWidth > 0 && _viewportCanvas != null && _sizingBorder != null && _dragBorder != null)
        {
            _sizingBorder.BorderThickness = _dragBorder.BorderThickness = new Thickness(
               _viewportCanvas.ActualWidth / ActualWidth * BorderThickness.Left,
               _viewportCanvas.ActualWidth / ActualWidth * BorderThickness.Top,
               _viewportCanvas.ActualWidth / ActualWidth * BorderThickness.Right,
               _viewportCanvas.ActualWidth / ActualWidth * BorderThickness.Bottom);
        }
    }

    /// <summary>
    /// Event raised on mouse down in the ZoomAndPanControl.
    /// </summary>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        ZoomAndPanControl.SaveZoom();
        _mouseHandlingMode = MouseHandlingMode.Panning;
        _origContentMouseDownPoint = e.GetPosition(_viewportCanvas);

        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
        {
            // Shift + left- or right-down initiates zooming mode.
            _mouseHandlingMode = MouseHandlingMode.DragZooming;

            if (_dragBorder != null)
                _dragBorder.Visibility = Visibility.Hidden;

            if (_sizingBorder != null)
                _sizingBorder.Visibility = Visibility.Visible;

            Canvas.SetLeft(_sizingBorder, _origContentMouseDownPoint.X);
            Canvas.SetTop(_sizingBorder, _origContentMouseDownPoint.Y);

            if (_sizingBorder != null)
            {
                _sizingBorder.Width = 0;
                _sizingBorder.Height = 0;
            }
        }
        else
        {
            // Just a plain old left-down initiates panning mode.
            _mouseHandlingMode = MouseHandlingMode.Panning;
        }

        if (_mouseHandlingMode != MouseHandlingMode.None)
        {
            // Capture the mouse so that we eventually receive the mouse up event.
            _viewportCanvas?.CaptureMouse();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Event raised on mouse up in the ZoomAndPanControl.
    /// </summary>
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (_mouseHandlingMode == MouseHandlingMode.DragZooming)
        {
            var zoomAndPanControl = ZoomAndPanControl;
            var curContentPoint = e.GetPosition(_viewportCanvas);
            if (_viewportCanvas is not null)
            {
                var rect = ViewportHelpers.Clip(curContentPoint, _origContentMouseDownPoint, new Point(0, 0),
                new Point(_viewportCanvas.Width, _viewportCanvas.Height));
                zoomAndPanControl.AnimatedZoomTo(rect);
            }
            if (_dragBorder != null)
                _dragBorder.Visibility = Visibility.Visible;
            if (_sizingBorder != null)
                _sizingBorder.Visibility = Visibility.Hidden;
        }
        _mouseHandlingMode = MouseHandlingMode.None;
        _viewportCanvas?.ReleaseMouseCapture();
        e.Handled = true;
    }

    /// <summary>
    /// Event raised on mouse move in the ZoomAndPanControl.
    /// </summary>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_mouseHandlingMode == MouseHandlingMode.Panning)
        {
            var curContentPoint = e.GetPosition(_viewportCanvas);
            var rectangleDragVector = curContentPoint - _origContentMouseDownPoint;
            //
            // When in 'dragging rectangles' mode update the position of the rectangle as the user drags it.
            //
            _origContentMouseDownPoint = e.GetPosition(_viewportCanvas).Clamp();
            Canvas.SetLeft(_dragBorder, Canvas.GetLeft(_dragBorder) + rectangleDragVector.X);
            Canvas.SetTop(_dragBorder, Canvas.GetTop(_dragBorder) + rectangleDragVector.Y);
        }
        else if (_mouseHandlingMode == MouseHandlingMode.DragZooming)
        {
            var curContentPoint = e.GetPosition(_viewportCanvas);
            if (_viewportCanvas is not null)
            {
                var rect = ViewportHelpers.Clip(curContentPoint, _origContentMouseDownPoint, new Point(0, 0), new Point(_viewportCanvas.Width, _viewportCanvas.Height));
                if (_sizingBorder is not null)
                    ViewportHelpers.PositionBorderOnCanvas(_sizingBorder, rect);
            }
        }

        e.Handled = true;
    }

    /// <summary>
    /// Event raised with the double click command
    /// </summary>
    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
        {
            var zoomAndPanControl = ZoomAndPanControl;
            zoomAndPanControl.SaveZoom();
            zoomAndPanControl.AnimatedSnapTo(e.GetPosition(_viewportCanvas));
        }
    }
    #endregion

    #region Methods

    private void OnVisualChanged(DependencyPropertyChangedEventArgs e) => SetBackground(e.NewValue as FrameworkElement);

    private void SetBackground(FrameworkElement? frameworkElement)
    {
        var visualBrush = new VisualBrush
        {
            Visual = frameworkElement,
            ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
            ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
            TileMode = TileMode.None,
            Stretch = Stretch.None
        };

        if (frameworkElement != null)
        {
            frameworkElement.SizeChanged += (s, e) =>
            {
                if (_viewportCanvas != null)
                {
                    _viewportCanvas.Height = frameworkElement.ActualHeight;
                    _viewportCanvas.Width = frameworkElement.ActualWidth;
                    _viewportCanvas.Background = visualBrush;
                }
            };
        }
    }

    #endregion
}
