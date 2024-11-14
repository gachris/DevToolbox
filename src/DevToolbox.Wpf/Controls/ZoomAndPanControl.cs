using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DevToolbox.Wpf.Data;
using DevToolbox.Wpf.Documents;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Helpers;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A control that provides zooming and panning functionality for its content. 
/// It supports various zooming methods like "fit", "fill", and "zoom-to-rect", 
/// as well as mouse and keyboard input for controlling the zoom level and content offset.
/// </summary>
[TemplatePart(Name = PART_Content, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PART_DragZoomBorder, Type = typeof(Border))]
[TemplatePart(Name = PART_DragZoomCanvas, Type = typeof(Canvas))]
public class ZoomAndPanControl : ContentControl, IScrollInfo
{
    private enum CurrentZoomType
    {
        Fill, Fit, Other
    }

    #region Fields/Consts

    /// <summary>
    /// The name of the content part used for zooming and panning operations.
    /// </summary>
    protected const string PART_Content = nameof(PART_Content);

    /// <summary>
    /// The name of the border element used for dragging the zoom rectangle.
    /// </summary>
    protected const string PART_DragZoomBorder = nameof(PART_DragZoomBorder);

    /// <summary>
    /// The name of the canvas element used for displaying the zoom rectangle during drag operations.
    /// </summary>
    protected const string PART_DragZoomCanvas = nameof(PART_DragZoomCanvas);

    /// <summary>
    /// Event raised when the ViewportZoom property has changed.
    /// </summary>
    public event PropertyChangedCallback? ViewportZoomChanged;

    /// <summary>
    /// Event raised when the ContentOffsetX property has changed.
    /// </summary>
    public event EventHandler? ContentOffsetXChanged;

    /// <summary>
    /// Event raised when the ContentOffsetY property has changed.
    /// </summary>
    public event EventHandler? ContentOffsetYChanged;

    /// <summary>
    /// Event raised when the ViewportZoom property has changed.
    /// </summary>
    public event EventHandler? ContentZoomChanged;

    /// <summary>
    /// Timer for handling zoom/pan keep-alive behavior (750ms interval).
    /// </summary>
    private KeepAliveTimer? _timer750Miliseconds;

    /// <summary>
    /// Timer for handling zoom/pan keep-alive behavior (1500ms interval).
    /// </summary>
    private KeepAliveTimer? _timer1500Miliseconds;

    /// <summary>
    /// Cached state for undoing/redoing viewport zoom changes.
    /// </summary>
    private UndoRedoStackItem? _viewportZoomCache;

    /// <summary>
    /// Stack for storing undoable zoom actions.
    /// </summary>
    private readonly Stack<UndoRedoStackItem> _undoStack = new();

    /// <summary>
    /// Stack for storing redoable zoom actions.
    /// </summary>
    private readonly Stack<UndoRedoStackItem> _redoStack = new();

    /// <summary>
    /// Records the unscaled extent of the content.
    /// This is calculated during the measure and arrange.
    /// </summary>
    private Size _unScaledExtent = new(0, 0);

    /// <summary>
    /// Records the size of the viewport (in viewport coordinates) onto the content.
    /// This is calculated during the measure and arrange.
    /// </summary>
    private Size _viewport = new(0, 0);

    /// <summary>
    /// Enum to track the current zoom behavior.
    /// </summary>
    private CurrentZoomType _currentZoomTypeEnum;

    /// <summary>
    /// Adorner used for cropping behavior within the control.
    /// </summary>
    private CroppingAdorner? _croppingAdorner;

    /// <summary>
    /// Reference to the underlying content, which is named PART_Content in the template.
    /// </summary>
    private ContentPresenter? _content;

    /// <summary>
    /// The transform that is applied to the content to scale it by 'ViewportZoom'.
    /// </summary>
    private ScaleTransform? _contentZoomTransform;

    /// <summary>
    /// The transform that is applied to the content to offset it by 'ContentOffsetX' and 'ContentOffsetY'.
    /// </summary>
    private TranslateTransform? _contentOffsetTransform;

    /// <summary>
    /// The height of the viewport in content coordinates, clamped to the height of the content.
    /// </summary>
    private double _constrainedContentViewportHeight;

    /// <summary>
    /// The width of the viewport in content coordinates, clamped to the width of the content.
    /// </summary>
    private double _constrainedContentViewportWidth;

    /// <summary>
    /// Normally when content offsets changes the content focus is automatically updated.
    /// This syncronization is disabled when 'disableContentFocusSync' is set to 'true'.
    /// When we are zooming in or out we 'disableContentFocusSync' is set to 'true' because 
    /// we are zooming in or out relative to the content focus we don't want to update the focus.
    /// </summary>
    private bool _disableContentFocusSync;

    /// <summary>
    /// Enable the update of the content offset as the content scale changes.
    /// This enabled for zooming about a point (google-maps style zooming) and zooming to a rect.
    /// </summary>
    private bool _enableContentOffsetUpdateFromScale;

    /// <summary>
    /// Used to disable syncronization between IScrollInfo interface and ContentOffsetX/ContentOffsetY.
    /// </summary>
    private bool _disableScrollOffsetSync;

    /// <summary>
    /// The control for creating a zoom border
    /// </summary>
    private Border? _partDragZoomBorder;

    /// <summary>
    /// The control for containing a zoom border
    /// </summary>
    private Canvas? _partDragZoomCanvas;

    /// <summary>
    /// Specifies the current state of the mouse handling logic.
    /// </summary>
    private MouseHandlingMode _mouseHandlingMode = MouseHandlingMode.None;

    /// <summary>
    /// The point that was clicked relative to the ZoomAndPanControl.
    /// </summary>
    private Point _origZoomAndPanControlMouseDownPoint;

    /// <summary>
    /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
    /// </summary>
    private Point _origContentMouseDownPoint;

    /// <summary>
    /// Records which mouse button clicked during mouse dragging.
    /// </summary>
    private MouseButton _mouseButtonDown;

    /// <summary>
    /// DependencyProperty for managing the internal zoom level of the viewport.
    /// Provides change and coercion callbacks for custom behavior.
    /// </summary>
    private static readonly DependencyProperty InternalViewportZoomProperty =
        DependencyProperty.Register(nameof(InternalViewportZoom), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(1.0, (d, e) => ((ZoomAndPanControl)d).OnInternalViewportZoomChanged(e), (d, baseValue) => ((ZoomAndPanControl)d).OnInternalViewportZoomCoerce(baseValue)));

    /// <summary>
    /// DependencyProperty for the zoom level of the viewport.
    /// </summary>
    public static readonly DependencyProperty ViewportZoomProperty =
        DependencyProperty.Register(nameof(ViewportZoom), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(1.0d, (d, e) => ((ZoomAndPanControl)d).OnViewportZoomChanged(e)));

    /// <summary>
    /// DependencyProperty for the style of the drag-zoom rectangle.
    /// </summary>
    public static readonly DependencyProperty DragZoomRectangleStyleProperty =
        DependencyProperty.Register(nameof(DragZoomRectangleStyle), typeof(Style), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// DependencyProperty for the background brush of the outer area of the zoomable content.
    /// </summary>
    public static readonly DependencyProperty OuterBackgroundProperty =
        DependencyProperty.Register(nameof(OuterBackground), typeof(Brush), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default, (d, e) => ((ZoomAndPanControl)d).OuterBackgroundChanged(e)));

    /// <summary>
    /// DependencyProperty for controlling the duration of zoom animations.
    /// </summary>
    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.4d));

    /// <summary>
    /// DependencyProperty for the initial position of the zoom and pan control.
    /// </summary>
    public static readonly DependencyProperty ZoomAndPanInitialPositionProperty =
        DependencyProperty.Register(nameof(ZoomAndPanInitialPosition), typeof(ZoomAndPanInitialPositionType), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(ZoomAndPanInitialPositionType.Default));

    /// <summary>
    /// DependencyProperty for the horizontal offset of the content.
    /// Includes change and coercion callbacks.
    /// </summary>
    public static readonly DependencyProperty ContentOffsetXProperty =
        DependencyProperty.Register(nameof(ContentOffsetX), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default(double), (d, e) => ((ZoomAndPanControl)d).OnContentOffsetXChanged(e), (d, baseValue) => ((ZoomAndPanControl)d).OnContentOffsetXCoerce(baseValue)));

    /// <summary>
    /// DependencyProperty for the vertical offset of the content.
    /// Includes change and coercion callbacks.
    /// </summary>
    public static readonly DependencyProperty ContentOffsetYProperty =
        DependencyProperty.Register(nameof(ContentOffsetY), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default(double), (d, e) => ((ZoomAndPanControl)d).OnContentOffsetYChanged(e), (d, baseValue) => ((ZoomAndPanControl)d).OnContentOffsetYCoerce(baseValue)));

    /// <summary>
    /// DependencyProperty for the height of the content viewport.
    /// </summary>
    public static readonly DependencyProperty ContentViewportHeightProperty =
        DependencyProperty.Register(nameof(ContentViewportHeight), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default(double)));

    /// <summary>
    /// DependencyProperty for the width of the content viewport.
    /// </summary>
    public static readonly DependencyProperty ContentViewportWidthProperty =
        DependencyProperty.Register(nameof(ContentViewportWidth), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default(double)));

    /// <summary>
    /// DependencyProperty for the horizontal zoom focus within the content.
    /// </summary>
    public static readonly DependencyProperty ContentZoomFocusXProperty =
        DependencyProperty.Register(nameof(ContentZoomFocusX), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default(double)));

    /// <summary>
    /// DependencyProperty for the vertical zoom focus within the content.
    /// </summary>
    public static readonly DependencyProperty ContentZoomFocusYProperty =
        DependencyProperty.Register(nameof(ContentZoomFocusY), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default(double)));

    /// <summary>
    /// DependencyProperty to enable or disable mouse wheel scrolling.
    /// </summary>
    public static readonly DependencyProperty IsMouseWheelScrollingEnabledProperty =
        DependencyProperty.Register(nameof(IsMouseWheelScrollingEnabled), typeof(bool), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default(bool)));

    /// <summary>
    /// DependencyProperty for the type of minimum zoom (e.g., fixed or adaptive).
    /// </summary>
    public static readonly DependencyProperty MinimumZoomTypeProperty =
        DependencyProperty.Register(nameof(MinimumZoomType), typeof(MinimumZoomType), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(MinimumZoomType.MinimumZoom));

    /// <summary>
    /// DependencyProperty for the minimum zoom level.
    /// Includes a change callback to handle changes in the minimum zoom.
    /// </summary>
    public static readonly DependencyProperty MinimumZoomProperty =
        DependencyProperty.Register(nameof(MinimumZoom), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.1, (d, e) => ((ZoomAndPanControl)d).OnMinimumZoomChanged(e)));

    /// <summary>
    /// DependencyProperty for the maximum zoom level.
    /// Includes a change callback to handle changes in the maximum zoom.
    /// </summary>
    public static readonly DependencyProperty MaximumZoomProperty =
        DependencyProperty.Register(nameof(MaximumZoom), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(10.0, (d, e) => ((ZoomAndPanControl)d).OnMaximumZoomChanged(e)));

    /// <summary>
    /// DependencyProperty for tracking the current position of the mouse within the control.
    /// </summary>
    public static readonly DependencyProperty MousePositionProperty =
        DependencyProperty.Register(nameof(MousePosition), typeof(Point), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default(Point), (d, e) => ((ZoomAndPanControl)d).OnMousePositionChanged(e)));

    /// <summary>
    /// DependencyProperty for enabling or disabling animations in zoom and pan operations.
    /// </summary>
    public static readonly DependencyProperty UseAnimationsProperty =
        DependencyProperty.Register(nameof(UseAnimations), typeof(bool), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// DependencyProperty for the horizontal zoom focus point in the viewport.
    /// </summary>
    public static readonly DependencyProperty ViewportZoomFocusXProperty =
        DependencyProperty.Register(nameof(ViewportZoomFocusX), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default(double)));

    /// <summary>
    /// DependencyProperty for the vertical zoom focus point in the viewport.
    /// </summary>
    public static readonly DependencyProperty ViewportZoomFocusYProperty =
        DependencyProperty.Register(nameof(ViewportZoomFocusY), typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(default(double)));

    /// <summary>
    /// Read-only DependencyProperty for the clamped minimum zoom level.
    /// </summary>
    private static readonly DependencyPropertyKey MinimumZoomClampedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(MinimumZoomClamped), typeof(double), typeof(ZoomAndPanControl), new PropertyMetadata(default(double)));

    /// <summary>
    /// DependencyProperty for the clamped minimum zoom level, which is read-only.
    /// This property ensures that the minimum zoom level doesn't go below a certain threshold.
    /// </summary>
    public static readonly DependencyProperty MinimumZoomClampedProperty = MinimumZoomClampedPropertyKey.DependencyProperty;

    /// <summary>
    /// RoutedCommand for fitting the content to the viewport.
    /// </summary>
    public static readonly RoutedCommand FitCommand = new(nameof(FitCommand), typeof(ZoomAndPanControl));

    /// <summary>
    /// RoutedCommand for filling the viewport with the content.
    /// </summary>
    public static readonly RoutedCommand FillCommand = new(nameof(FillCommand), typeof(ZoomAndPanControl));

    /// <summary>
    /// RoutedCommand for zooming in.
    /// </summary>
    public static readonly RoutedCommand ZoomInCommand = new(nameof(ZoomInCommand), typeof(ZoomAndPanControl));

    /// <summary>
    /// RoutedCommand for zooming out.
    /// </summary>
    public static readonly RoutedCommand ZoomOutCommand = new(nameof(ZoomOutCommand), typeof(ZoomAndPanControl));

    /// <summary>
    /// RoutedCommand for undoing the last zoom action.
    /// </summary>
    public static readonly RoutedCommand UndoZoomCommand = new(nameof(UndoZoomCommand), typeof(ZoomAndPanControl));

    /// <summary>
    /// RoutedCommand for redoing the last undone zoom action.
    /// </summary>
    public static readonly RoutedCommand RedoZoomCommand = new(nameof(RedoZoomCommand), typeof(ZoomAndPanControl));

    /// <summary>
    /// RoutedCommand for setting the zoom level to a specific percentage.
    /// </summary>
    public static readonly RoutedCommand ZoomPercentCommand = new(nameof(ZoomPercentCommand), typeof(ZoomAndPanControl));

    /// <summary>
    /// RoutedCommand for zooming based on a ratio from the minimum zoom level.
    /// </summary>
    public static readonly RoutedCommand ZoomRatioFromMinimumCommand = new(nameof(ZoomRatioFromMinimumCommand), typeof(ZoomAndPanControl));

    #endregion

    #region Properties

    /// <summary>
    /// Set to 'true' when the vertical scrollbar is enabled.
    /// </summary>
    public bool CanVerticallyScroll { get; set; }

    /// <summary>
    /// Set to 'true' when the vertical scrollbar is enabled.
    /// </summary>
    public bool CanHorizontallyScroll { get; set; }

    /// <summary>
    /// The width of the content (with 'ViewportZoom' applied).
    /// </summary>
    public double ExtentWidth => _unScaledExtent.Width * InternalViewportZoom;

    /// <summary>
    /// The height of the content (with 'ViewportZoom' applied).
    /// </summary>
    public double ExtentHeight => _unScaledExtent.Height * InternalViewportZoom;

    /// <summary>
    /// Get the width of the viewport onto the content.
    /// </summary>
    public double ViewportWidth => _viewport.Width;

    /// <summary>
    /// Get the height of the viewport onto the content.
    /// </summary>
    public double ViewportHeight => _viewport.Height;

    /// <summary>
    /// Reference to the ScrollViewer that is wrapped (in XAML) around the ZoomAndPanControl.
    /// Or set to null if there is no ScrollViewer.
    /// </summary>
    public ScrollViewer? ScrollOwner { get; set; }

    /// <summary>
    /// The offset of the horizontal scrollbar.
    /// </summary>
    public double HorizontalOffset => ContentOffsetX * InternalViewportZoom;

    /// <summary>
    /// The offset of the vertical scrollbar.
    /// </summary>
    public double VerticalOffset => ContentOffsetY * InternalViewportZoom;
    /// <summary>
    /// Indicates whether there are any zoom actions that can be undone, based on the undo stack.
    /// </summary>
    private bool CanUndoZoom => _undoStack.Any();

    /// <summary>
    /// Indicates whether there are any zoom actions that can be redone, based on the redo stack.
    /// </summary>
    private bool CanRedoZoom => _redoStack.Any();

    /// <summary>
    /// Determines if the viewport can be zoomed to fill the available area.
    /// Returns true if the current zoom level is not within one percent of the fill zoom value 
    /// and if the fill zoom value is greater than or equal to the minimum zoom clamped.
    /// </summary>
    private bool CanFill => !InternalViewportZoom.IsWithinOnePercent(FillZoomValue) && FillZoomValue >= MinimumZoomClamped;

    /// <summary>
    /// Determines if the viewport can be zoomed to fit the available area.
    /// Returns true if the current zoom level is not within one percent of the fit zoom value 
    /// and if the fit zoom value is greater than or equal to the minimum zoom clamped.
    /// </summary>
    public bool CanFit => !InternalViewportZoom.IsWithinOnePercent(FitZoomValue) && FitZoomValue >= MinimumZoomClamped;

    /// <summary>
    /// Calculates the zoom value required to fit the content within the viewport dimensions.
    /// </summary>
    public double FitZoomValue => ViewportHelpers.FitZoom(ActualWidth, ActualHeight, _content?.ActualWidth, _content?.ActualHeight);

    /// <summary>
    /// Calculates the zoom value required to fill the entire viewport with the content.
    /// </summary>
    public double FillZoomValue => ViewportHelpers.FillZoom(ActualWidth, ActualHeight, _content?.ActualWidth, _content?.ActualHeight);

    /// <summary>
    /// Gets or sets the clamped minimum zoom level.
    /// The value is read from the MinimumZoomClampedProperty and can only be set privately.
    /// </summary>
    public double MinimumZoomClamped
    {
        get => (double)GetValue(MinimumZoomClampedProperty);
        private set => SetValue(MinimumZoomClampedPropertyKey, value);
    }

    /// <summary>
    /// Gets or sets the style for the zoom rectangle displayed when dragging to zoom.
    /// </summary>
    public Style DragZoomRectangleStyle
    {
        get => (Style)GetValue(DragZoomRectangleStyleProperty);
        set => SetValue(DragZoomRectangleStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the internal viewport zoom level, a private property that tracks zoom changes within the viewport.
    /// </summary>
    private double InternalViewportZoom
    {
        get => (double)GetValue(InternalViewportZoomProperty);
        set => SetValue(InternalViewportZoomProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to paint the outer background of the zoom and pan control.
    /// </summary>
    public Brush OuterBackground
    {
        get => (Brush)GetValue(OuterBackgroundProperty);
        set => SetValue(OuterBackgroundProperty, value);
    }

    /// <summary>
    /// The duration of the animations (in seconds) started by calling AnimatedZoomTo and the other animation methods.
    /// </summary>
    public double AnimationDuration
    {
        get => (double)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>
    /// The duration of the animations (in seconds) started by calling AnimatedZoomTo and the other animation methods.
    /// </summary>
    public ZoomAndPanInitialPositionType ZoomAndPanInitialPosition
    {
        get => (ZoomAndPanInitialPositionType)GetValue(ZoomAndPanInitialPositionProperty);
        set => SetValue(ZoomAndPanInitialPositionProperty, value);
    }

    /// <summary>
    /// Get/set the X offset (in content coordinates) of the view on the content.
    /// </summary>
    public double ContentOffsetX
    {
        get => (double)GetValue(ContentOffsetXProperty);
        set => SetValue(ContentOffsetXProperty, value);
    }

    /// <summary>
    /// Get/set the Y offset (in content coordinates) of the view on the content.
    /// </summary>
    public double ContentOffsetY
    {
        get => (double)GetValue(ContentOffsetYProperty);
        set => SetValue(ContentOffsetYProperty, value);
    }

    /// <summary>
    /// Get the viewport height, in content coordinates.
    /// </summary>
    public double ContentViewportHeight
    {
        get => (double)GetValue(ContentViewportHeightProperty);
        set => SetValue(ContentViewportHeightProperty, value);
    }

    /// <summary>
    /// Get the viewport width, in content coordinates.
    /// </summary>
    public double ContentViewportWidth
    {
        get => (double)GetValue(ContentViewportWidthProperty);
        set => SetValue(ContentViewportWidthProperty, value);
    }

    /// <summary>
    /// The X coordinate of the content focus, this is the point that we are focusing on when zooming.
    /// </summary>
    public double ContentZoomFocusX
    {
        get => (double)GetValue(ContentZoomFocusXProperty);
        set => SetValue(ContentZoomFocusXProperty, value);
    }

    /// <summary>
    /// The Y coordinate of the content focus, this is the point that we are focusing on when zooming.
    /// </summary>
    public double ContentZoomFocusY
    {
        get => (double)GetValue(ContentZoomFocusYProperty);
        set => SetValue(ContentZoomFocusYProperty, value);
    }

    /// <summary>
    /// Set to 'true' to enable the mouse wheel to scroll the zoom and pan control.
    /// This is set to 'false' by default.
    /// </summary>
    public bool IsMouseWheelScrollingEnabled
    {
        get => (bool)GetValue(IsMouseWheelScrollingEnabledProperty);
        set => SetValue(IsMouseWheelScrollingEnabledProperty, value);
    }

    /// <summary>
    /// Get/set the maximum value for 'ViewportZoom'.
    /// </summary>
    public MinimumZoomType MinimumZoomType
    {
        get => (MinimumZoomType)GetValue(MinimumZoomTypeProperty);
        set => SetValue(MinimumZoomTypeProperty, value);
    }

    /// <summary>
    /// Get/set the MinimumZoom value for 'ViewportZoom'.
    /// </summary>
    public double MinimumZoom
    {
        get => (double)GetValue(MinimumZoomProperty);
        set => SetValue(MinimumZoomProperty, value);
    }

    /// <summary>
    /// Get/set the maximum value for 'ViewportZoom'.
    /// </summary>
    public double MaximumZoom
    {
        get => (double)GetValue(MaximumZoomProperty);
        set => SetValue(MaximumZoomProperty, value);
    }

    /// <summary>
    /// Get/set the MinimumZoom value for 'ViewportZoom'.
    /// </summary>
    public Point MousePosition
    {
        get => (Point)GetValue(MousePositionProperty);
        set => SetValue(MousePositionProperty, value);
    }

    /// <summary>
    /// This is used for binding a slider to control the zoom. Cannot use the InternalUseAnimations because of all the 
    /// assumptions in when the this property is changed. THIS IS NOT USED FOR THE ANIMATIONS
    /// </summary>
    public bool UseAnimations
    {
        get => (bool)GetValue(UseAnimationsProperty);
        set => SetValue(UseAnimationsProperty, value);
    }

    /// <summary>
    /// This is used for binding a slider to control the zoom. Cannot use the InternalViewportZoom because of all the 
    /// assumptions in when the this property is changed. THIS IS NOT USED FOR THE ANIMATIONS
    /// </summary>
    public double ViewportZoom
    {
        get => (double)GetValue(ViewportZoomProperty);
        set => SetValue(ViewportZoomProperty, value);
    }

    /// <summary>
    /// The X coordinate of the viewport focus, this is the point in the viewport (in viewport coordinates) 
    /// that the content focus point is locked to while zooming in.
    /// </summary>
    public double ViewportZoomFocusX
    {
        get => (double)GetValue(ViewportZoomFocusXProperty);
        set => SetValue(ViewportZoomFocusXProperty, value);
    }

    /// <summary>
    /// The Y coordinate of the viewport focus, this is the point in the viewport (in viewport coordinates) 
    /// that the content focus point is locked to while zooming in.
    /// </summary>
    public double ViewportZoomFocusY
    {
        get => (double)GetValue(ViewportZoomFocusYProperty);
        set => SetValue(ViewportZoomFocusYProperty, value);
    }

    #endregion

    /// <summary>
    /// Static constructor to define metadata for the control (and link it to the style in Generic.xaml).
    /// </summary>
    static ZoomAndPanControl()
    {
        Type typeFromHandle = typeof(ZoomAndPanControl);

        DefaultStyleKeyProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(typeFromHandle));

        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(FillCommand, OnFillExecuted, OnFillCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(FitCommand, OnFitExecuted, OnFitCanExecute));

        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(ZoomPercentCommand, OnZoomPercentExecuted, OnZoomPercentCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(ZoomRatioFromMinimumCommand, OnZoomRatioFromMinimumExecuted, OnZoomRatioFromMinimumCanExecute));

        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(ZoomOutCommand, new KeyGesture(Key.OemMinus, ModifierKeys.Shift)));
        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(ZoomOutCommand, new KeyGesture(Key.Subtract)));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(ZoomOutCommand, OnZoomOutExecuted, OnZoomOutCanExecute));

        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(ZoomInCommand, new KeyGesture(Key.OemPlus, ModifierKeys.Shift)));
        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(ZoomInCommand, new KeyGesture(Key.Add)));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(ZoomInCommand, OnZoomInExecuted, OnZoomInCanExecute));

        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(UndoZoomCommand, new KeyGesture(Key.Z, ModifierKeys.Control)));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(UndoZoomCommand, OnUndoZoomExecuted, OnUndoZoomCanExecute));

        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(UndoZoomCommand, new KeyGesture(Key.Y, ModifierKeys.Control)));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(UndoZoomCommand, OnRedoZoomExecuted, OnRedoZoomCanExecute));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZoomAndPanControl"/> class.
    /// </summary>
    public ZoomAndPanControl()
    {
    }

    #region Methods Override

    /// <summary>
    /// Called when a template has been applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _content = Template.FindName(PART_Content, this) as ContentPresenter;
        if (_content != null)
        {
            _contentZoomTransform = new ScaleTransform(InternalViewportZoom, InternalViewportZoom);
            _contentOffsetTransform = new TranslateTransform();
            UpdateTranslationX();
            UpdateTranslationY();

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(_contentOffsetTransform);
            transformGroup.Children.Add(_contentZoomTransform);
            _content.RenderTransform = transformGroup;

            _partDragZoomBorder = Template.FindName(PART_DragZoomBorder, this) as Border;
            _partDragZoomCanvas = Template.FindName(PART_DragZoomCanvas, this) as Canvas;

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);

            if (adornerLayer == null)
                throw new ArgumentNullException(nameof(adornerLayer));

            if (_croppingAdorner != null)
                adornerLayer.Remove(_croppingAdorner);

            _croppingAdorner = new CroppingAdorner(this, new Rect())
            {
                Fill = OuterBackground,
                ShowCorners = false
            };

            adornerLayer.Add(_croppingAdorner);

            SizeChanged += (sender, e) => SetClippingRectangle();
        }
    }

    /// <summary>
    /// Need to update zoom values if size changes, and update ViewportZoom if too low
    /// </summary>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        if (sizeInfo.NewSize.Width <= 1 || sizeInfo.NewSize.Height <= 1)
            return;

        switch (_currentZoomTypeEnum)
        {
            case CurrentZoomType.Fit:
                InternalViewportZoom = ViewportHelpers.FitZoom(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height,
                    _content?.ActualWidth, _content?.ActualHeight);
                break;
            case CurrentZoomType.Fill:
                InternalViewportZoom = ViewportHelpers.FillZoom(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height,
                    _content?.ActualWidth, _content?.ActualHeight);
                break;
        }

        if (InternalViewportZoom < MinimumZoomClamped)
            InternalViewportZoom = MinimumZoomClamped;

        MinimumZoomClamped = ((MinimumZoomType == MinimumZoomType.FillScreen) ?
            FillZoomValue : (MinimumZoomType == MinimumZoomType.FitScreen) ?
            FitZoomValue : MinimumZoom).ToRealNumber();
    }

    /// <summary>
    /// Measure the control and it's children.
    /// </summary>
    protected override Size MeasureOverride(Size constraint)
    {
        var infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
        var childSize = base.MeasureOverride(infiniteSize);

        if (childSize != _unScaledExtent)
        {
            //
            // Use the size of the child as the un-scaled extent content.
            //
            _unScaledExtent = childSize;
            ScrollOwner?.InvalidateScrollInfo();
        }

        //
        // Update the size of the viewport onto the content based on the passed in 'constraint'.
        //
        UpdateViewportSize(constraint);
        var width = constraint.Width;
        var height = constraint.Height;
        if (double.IsInfinity(width)) width = childSize.Width;
        if (double.IsInfinity(height)) height = childSize.Height;
        UpdateTranslationX();
        UpdateTranslationY();
        return new Size(width, height);
    }

    /// <summary>
    /// Arrange the control and it's children.
    /// </summary>
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        var size = base.ArrangeOverride(DesiredSize);

        if (_content != null && _content.DesiredSize != _unScaledExtent)
        {
            //
            // Use the size of the child as the un-scaled extent content.
            //
            _unScaledExtent = _content.DesiredSize;
            ScrollOwner?.InvalidateScrollInfo();
        }

        //
        // Update the size of the viewport onto the content based on the passed in 'arrangeBounds'.
        //
        UpdateViewportSize(arrangeBounds);

        return size;
    }

    /// <summary>
    /// When content is renewed, set event to set the initial position as specified
    /// </summary>
    /// <param name="oldContent"></param>
    /// <param name="newContent"></param>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        if (oldContent is FrameworkElement oldElement)
            oldElement.SizeChanged -= SetZoomAndPanInitialPosition;

        if (newContent is FrameworkElement newElement)
            newElement.SizeChanged += SetZoomAndPanInitialPosition;
    }

    /// <summary>
    /// Event raised on mouse down in the ZoomAndPanControl.
    /// </summary>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        SaveZoom();
        _content?.Focus();
        Keyboard.Focus(_content);

        _mouseButtonDown = e.ChangedButton;
        _origZoomAndPanControlMouseDownPoint = e.GetPosition(this);
        _origContentMouseDownPoint = e.GetPosition(_content);

        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
            (e.ChangedButton == MouseButton.Left ||
             e.ChangedButton == MouseButton.Right))
        {
            // Shift + left- or right-down initiates zooming mode.
            _mouseHandlingMode = MouseHandlingMode.Zooming;
        }
        else if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 &&
            _mouseButtonDown == MouseButton.Left)
        {
            // Just a plain old left-down initiates panning mode.
            _mouseHandlingMode = MouseHandlingMode.Panning;
        }

        if (_mouseHandlingMode != MouseHandlingMode.None)
        {
            e.Handled = true;
            // Capture the mouse so that we eventually receive the mouse up event.
            CaptureMouse();
        }
    }

    /// <summary>
    /// Event raised on mouse up in the ZoomAndPanControl.
    /// </summary>
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (_mouseHandlingMode != MouseHandlingMode.None)
        {
            if (_mouseHandlingMode == MouseHandlingMode.Zooming)
            {
                if (_mouseButtonDown == MouseButton.Left)
                {
                    // Shift + left-click zooms in on the content.
                    ZoomIn(_origContentMouseDownPoint);
                }
                else if (_mouseButtonDown == MouseButton.Right)
                {
                    // Shift + left-click zooms out from the content.
                    ZoomOut(_origContentMouseDownPoint);
                }
            }
            else if (_mouseHandlingMode == MouseHandlingMode.DragZooming)
            {
                var finalContentMousePoint = e.GetPosition(_content);
                // When drag-zooming has finished we zoom in on the rectangle that was highlighted by the user.
                ApplyDragZoomRect(finalContentMousePoint);
            }

            ReleaseMouseCapture();
            _mouseHandlingMode = MouseHandlingMode.None;
        }
    }

    /// <summary>
    /// Event raised on mouse move in the ZoomAndPanControl.
    /// </summary>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_content is null)
        {
            return;
        }

        var oldContentMousePoint = MousePosition;
        var curContentMousePoint = e.GetPosition(_content);
        MousePosition = curContentMousePoint.FilterClamp(_content.ActualWidth - 1, _content.ActualHeight - 1) ?? new Point();
        OnPropertyChanged(new DependencyPropertyChangedEventArgs(MousePositionProperty, oldContentMousePoint,
            curContentMousePoint));

        if (_mouseHandlingMode == MouseHandlingMode.Panning)
        {
            //
            // The user is left-dragging the mouse.
            // Pan the viewport by the appropriate amount.
            //
            var dragOffset = curContentMousePoint - _origContentMouseDownPoint;

            ContentOffsetX -= dragOffset.X;
            ContentOffsetY -= dragOffset.Y;

            e.Handled = true;
        }
        else if (_mouseHandlingMode == MouseHandlingMode.Zooming)
        {
            var curZoomAndPanControlMousePoint = e.GetPosition(this);
            var dragOffset = curZoomAndPanControlMousePoint - _origZoomAndPanControlMouseDownPoint;
            double dragThreshold = 10;
            if (_mouseButtonDown == MouseButton.Left &&
                (Math.Abs(dragOffset.X) > dragThreshold ||
                 Math.Abs(dragOffset.Y) > dragThreshold))
            {
                //
                // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
                // initiate drag zooming mode where the user can drag out a rectangle to select the area
                // to zoom in on.
                //
                _mouseHandlingMode = MouseHandlingMode.DragZooming;
                InitDragZoomRect(_origContentMouseDownPoint, curContentMousePoint);
            }
        }
        else if (_mouseHandlingMode == MouseHandlingMode.DragZooming)
        {
            //
            // When in drag zooming mode continously update the position of the rectangle
            // that the user is dragging out.
            //
            curContentMousePoint = e.GetPosition(this);
            SetDragZoomRect(_origZoomAndPanControlMouseDownPoint, curContentMousePoint);
        }
    }

    /// <summary>
    /// Event raised on mouse wheel moved in the ZoomAndPanControl.
    /// </summary>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        DelayedSaveZoom750Miliseconds();
        e.Handled = true;

        if (e.Delta > 0)
            ZoomIn(e.GetPosition(_content));
        else if (e.Delta < 0)
            ZoomOut(e.GetPosition(_content));
    }

    /// <summary>
    /// Event raised with the double click command
    /// </summary>
    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
        {
            SaveZoom();
            AnimatedSnapTo(e.GetPosition(_content));
        }
    }

    #endregion

    #region Undu & Redu

    /// <summary> 
    /// Record the previous zoom level, so that we can return to it.
    /// </summary>
    public void SaveZoom()
    {
        _viewportZoomCache = CreateUndoRedoStackItem();
        if (_undoStack.Any() && _viewportZoomCache.Equals(_undoStack.Peek())) return;
        _undoStack.Push(_viewportZoomCache);
        _redoStack.Clear();
    }

    /// <summary> 
    ///  Record the last saved zoom level, so that we can return to it if no activity for 750 milliseconds
    /// </summary>
    public void DelayedSaveZoom750Miliseconds()
    {
        if (_timer750Miliseconds?.Running != true) _viewportZoomCache = CreateUndoRedoStackItem();
        (_timer750Miliseconds ??= new KeepAliveTimer(TimeSpan.FromMilliseconds(740), () =>
        {
            if (_viewportZoomCache is null || (_undoStack.Any() && _viewportZoomCache.Equals(_undoStack.Peek()))) return;
            _undoStack.Push(_viewportZoomCache);
            _redoStack.Clear();
        })).Nudge();
    }

    /// <summary> 
    ///  Record the last saved zoom level, so that we can return to it if no activity for 1550 milliseconds
    /// </summary>
    public void DelayedSaveZoom1500Miliseconds()
    {
        if (!_timer1500Miliseconds?.Running != true) _viewportZoomCache = CreateUndoRedoStackItem();
        (_timer1500Miliseconds ??= new KeepAliveTimer(TimeSpan.FromMilliseconds(1500), () =>
        {
            if (_viewportZoomCache is null || (_undoStack.Any() && _viewportZoomCache.Equals(_undoStack.Peek()))) return;
            _undoStack.Push(_viewportZoomCache);
            _redoStack.Clear();
        })).Nudge();
    }

    private UndoRedoStackItem CreateUndoRedoStackItem()
    {
        return new UndoRedoStackItem(
            ContentOffsetX,
            ContentOffsetY,
            ContentViewportWidth,
            ContentViewportHeight,
            InternalViewportZoom);
    }

    #endregion

    #region IScrollInfo

    /// <summary>
    /// Called when the offset of the horizontal scrollbar has been set.
    /// </summary>
    public void SetHorizontalOffset(double offset)
    {
        if (_disableScrollOffsetSync) return;

        try
        {
            _disableScrollOffsetSync = true;
            ContentOffsetX = offset / InternalViewportZoom;
            DelayedSaveZoom750Miliseconds();
        }
        finally
        {
            _disableScrollOffsetSync = false;
        }
    }

    /// <summary>
    /// Called when the offset of the vertical scrollbar has been set.
    /// </summary>
    public void SetVerticalOffset(double offset)
    {
        if (_disableScrollOffsetSync) return;

        try
        {
            _disableScrollOffsetSync = true;
            ContentOffsetY = offset / InternalViewportZoom;
            DelayedSaveZoom750Miliseconds();
        }
        finally
        {
            _disableScrollOffsetSync = false;
        }
    }

    /// <summary>
    /// Shift the content offset one line up.
    /// </summary>
    public void LineUp()
    {
        DelayedSaveZoom750Miliseconds();
        ContentOffsetY -= (ContentViewportHeight / 10);
    }

    /// <summary>
    /// Shift the content offset one line down.
    /// </summary>
    public void LineDown()
    {
        DelayedSaveZoom750Miliseconds();
        ContentOffsetY += (ContentViewportHeight / 10);
    }

    /// <summary>
    /// Shift the content offset one line left.
    /// </summary>
    public void LineLeft()
    {
        DelayedSaveZoom750Miliseconds();
        ContentOffsetX -= (ContentViewportWidth / 10);
    }

    /// <summary>
    /// Shift the content offset one line right.
    /// </summary>
    public void LineRight()
    {
        DelayedSaveZoom750Miliseconds();
        ContentOffsetX += (ContentViewportWidth / 10);
    }

    /// <summary>
    /// Shift the content offset one page up.
    /// </summary>
    public void PageUp()
    {
        DelayedSaveZoom1500Miliseconds();
        ContentOffsetY -= ContentViewportHeight;
    }

    /// <summary>
    /// Shift the content offset one page down.
    /// </summary>
    public void PageDown()
    {
        DelayedSaveZoom1500Miliseconds();
        ContentOffsetY += ContentViewportHeight;
    }

    /// <summary>
    /// Shift the content offset one page left.
    /// </summary>
    public void PageLeft()
    {
        DelayedSaveZoom1500Miliseconds();
        ContentOffsetX -= ContentViewportWidth;
    }

    /// <summary>
    /// Shift the content offset one page right.
    /// </summary>
    public void PageRight()
    {
        DelayedSaveZoom1500Miliseconds();
        ContentOffsetX += ContentViewportWidth;
    }

    /// <summary>
    /// Don't handle mouse wheel input from the ScrollViewer, the mouse wheel is
    /// used for zooming in and out, not for manipulating the scrollbars.
    /// </summary>
    public void MouseWheelDown()
    {
        if (IsMouseWheelScrollingEnabled)
        {
            LineDown();
        }
    }

    /// <summary>
    /// Don't handle mouse wheel input from the ScrollViewer, the mouse wheel is
    /// used for zooming in and out, not for manipulating the scrollbars.
    /// </summary>
    public void MouseWheelLeft()
    {
        if (IsMouseWheelScrollingEnabled)
        {
            LineLeft();
        }
    }

    /// <summary>
    /// Don't handle mouse wheel input from the ScrollViewer, the mouse wheel is
    /// used for zooming in and out, not for manipulating the scrollbars.
    /// </summary>
    public void MouseWheelRight()
    {
        if (IsMouseWheelScrollingEnabled)
        {
            LineRight();
        }
    }

    /// <summary>
    /// Don't handle mouse wheel input from the ScrollViewer, the mouse wheel is
    /// used for zooming in and out, not for manipulating the scrollbars.
    /// </summary>
    public void MouseWheelUp()
    {
        if (IsMouseWheelScrollingEnabled)
        {
            LineUp();
        }
    }

    /// <summary>
    /// Bring the specified rectangle to view.
    /// </summary>
    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
        if (_content != null && _content.IsAncestorOf(visual))
        {
            var transformedRect = visual.TransformToAncestor(_content).TransformBounds(rectangle);
            var viewportRect = new Rect(ContentOffsetX, ContentOffsetY, ContentViewportWidth, ContentViewportHeight);
            if (!transformedRect.Contains(viewportRect))
            {
                double horizOffset = 0;
                double vertOffset = 0;

                if (transformedRect.Left < viewportRect.Left)
                {
                    //
                    // Want to move viewport left.
                    //
                    horizOffset = transformedRect.Left - viewportRect.Left;
                }
                else if (transformedRect.Right > viewportRect.Right)
                {
                    //
                    // Want to move viewport right.
                    //
                    horizOffset = transformedRect.Right - viewportRect.Right;
                }

                if (transformedRect.Top < viewportRect.Top)
                {
                    //
                    // Want to move viewport up.
                    //
                    vertOffset = transformedRect.Top - viewportRect.Top;
                }
                else if (transformedRect.Bottom > viewportRect.Bottom)
                {
                    //
                    // Want to move viewport down.
                    //
                    vertOffset = transformedRect.Bottom - viewportRect.Bottom;
                }

                SnapContentOffsetTo(new Point(ContentOffsetX + horizOffset, ContentOffsetY + vertOffset));
            }
        }
        return rectangle;
    }

    #endregion

    #region Methods

    private void SetScrollViewerFocus()
    {
        var scrollViewer = _content?.FindVisualAncestor<ScrollViewer>();
        if (scrollViewer != null)
        {
            Keyboard.Focus(scrollViewer);
            scrollViewer.Focus();
        }
    }

    /// <summary>
    /// Do an animated zoom to view a specific scale and rectangle (in content coordinates).
    /// </summary>
    public void AnimatedZoomTo(double newScale, Rect contentRect) => AnimatedZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)),
            delegate
            {
                //
                // At the end of the animation, ensure that we are snapped to the specified content offset.
                // Due to zooming in on the content focus point and rounding errors, the content offset may
                // be slightly off what we want at the end of the animation and this bit of code corrects it.
                //
                ContentOffsetX = contentRect.X;
                ContentOffsetY = contentRect.Y;
            });

    /// <summary>
    /// Do an animated zoom to the specified rectangle (in content coordinates).
    /// </summary>
    public void AnimatedZoomTo(Rect contentRect)
    {
        var scaleX = ContentViewportWidth / contentRect.Width;
        var scaleY = ContentViewportHeight / contentRect.Height;
        var contentFitZoom = InternalViewportZoom * Math.Min(scaleX, scaleY);
        AnimatedZoomPointToViewportCenter(contentFitZoom, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)), null);
    }

    /// <summary>
    /// Instantly zoom to the specified rectangle (in content coordinates).
    /// </summary>
    public void ZoomTo(Rect contentRect)
    {
        var scaleX = ContentViewportWidth / contentRect.Width;
        var scaleY = ContentViewportHeight / contentRect.Height;
        var newScale = InternalViewportZoom * Math.Min(scaleX, scaleY);

        ZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)));
    }

    /// <summary>
    /// Instantly center the view on the specified point (in content coordinates).
    /// </summary>
    public void SnapContentOffsetTo(Point contentOffset)
    {
        AnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
        AnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

        ContentOffsetX = contentOffset.X;
        ContentOffsetY = contentOffset.Y;
    }

    /// <summary>
    /// Instantly center the view on the specified point (in content coordinates).
    /// </summary>
    public void SnapTo(Point contentPoint)
    {
        AnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
        AnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

        ContentOffsetX = contentPoint.X - (ContentViewportWidth / 2);
        ContentOffsetY = contentPoint.Y - (ContentViewportHeight / 2);
    }

    /// <summary>
    /// Use animation to center the view on the specified point (in content coordinates).
    /// </summary>
    public void AnimatedSnapTo(Point contentPoint)
    {
        var newX = contentPoint.X - (ContentViewportWidth / 2);
        var newY = contentPoint.Y - (ContentViewportHeight / 2);

        AnimationHelper.StartAnimation(this, ContentOffsetXProperty, newX, AnimationDuration, UseAnimations);
        AnimationHelper.StartAnimation(this, ContentOffsetYProperty, newY, AnimationDuration, UseAnimations);
    }

    /// <summary>
    /// Zoom in/out centered on the specified point (in content coordinates).
    /// The focus point is kept locked to it's on screen position (ala google maps).
    /// </summary>
    public void AnimatedZoomAboutPoint(double newContentZoom, Point contentZoomFocus)
    {
        newContentZoom = Math.Min(Math.Max(newContentZoom, MinimumZoomClamped), MaximumZoom);

        AnimationHelper.CancelAnimation(this, ContentZoomFocusXProperty);
        AnimationHelper.CancelAnimation(this, ContentZoomFocusYProperty);
        AnimationHelper.CancelAnimation(this, ViewportZoomFocusXProperty);
        AnimationHelper.CancelAnimation(this, ViewportZoomFocusYProperty);

        ContentZoomFocusX = contentZoomFocus.X;
        ContentZoomFocusY = contentZoomFocus.Y;
        ViewportZoomFocusX = (ContentZoomFocusX - ContentOffsetX) * InternalViewportZoom;
        ViewportZoomFocusY = (ContentZoomFocusY - ContentOffsetY) * InternalViewportZoom;

        //
        // When zooming about a point make updates to ViewportZoom also update content offset.
        //
        _enableContentOffsetUpdateFromScale = true;

        AnimationHelper.StartAnimation(this, InternalViewportZoomProperty, newContentZoom, AnimationDuration,
            (sender, e) =>
            {
                _enableContentOffsetUpdateFromScale = false;
                ResetViewportZoomFocus();
            }, UseAnimations);
    }

    /// <summary>
    /// Zoom in/out centered on the specified point (in content coordinates).
    /// The focus point is kept locked to it's on screen position (ala google maps).
    /// </summary>
    public void ZoomAboutPoint(double newContentZoom, Point contentZoomFocus)
    {
        newContentZoom = Math.Min(Math.Max(newContentZoom, MinimumZoomClamped), MaximumZoom);

        var screenSpaceZoomOffsetX = (contentZoomFocus.X - ContentOffsetX) * InternalViewportZoom;
        var screenSpaceZoomOffsetY = (contentZoomFocus.Y - ContentOffsetY) * InternalViewportZoom;
        var contentSpaceZoomOffsetX = screenSpaceZoomOffsetX / newContentZoom;
        var contentSpaceZoomOffsetY = screenSpaceZoomOffsetY / newContentZoom;
        var newContentOffsetX = contentZoomFocus.X - contentSpaceZoomOffsetX;
        var newContentOffsetY = contentZoomFocus.Y - contentSpaceZoomOffsetY;

        AnimationHelper.CancelAnimation(this, InternalViewportZoomProperty);
        AnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
        AnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

        InternalViewportZoom = newContentZoom;
        ContentOffsetX = newContentOffsetX;
        ContentOffsetY = newContentOffsetY;
    }

    /// <summary>
    /// Zoom in/out centered on the viewport center.
    /// </summary>
    public void AnimatedZoomTo(double viewportZoom)
    {
        if (_content is null)
        {
            return;
        }

        var xadjust = (ContentViewportWidth - _content.ActualWidth) * InternalViewportZoom / 2;
        var yadjust = (ContentViewportHeight - _content.ActualHeight) * InternalViewportZoom / 2;
        var zoomCenter = (InternalViewportZoom >= FillZoomValue)
            ? new Point(ContentOffsetX + (ContentViewportWidth / 2), ContentOffsetY + (ContentViewportHeight / 2))
            : new Point(_content.ActualWidth / 2 - xadjust, _content.ActualHeight / 2 + yadjust);
        AnimatedZoomAboutPoint(viewportZoom, zoomCenter);
    }

    /// <summary>
    /// Zoom in/out centered on the viewport center.
    /// </summary>
    public void AnimatedZoomToCentered(double viewportZoom)
    {
        var zoomCenter = new Point(_content?.ActualWidth ?? 0 / 2, _content?.ActualHeight ?? 0 / 2);
        AnimatedZoomAboutPoint(viewportZoom, zoomCenter);
    }

    /// <summary>
    /// Zoom in/out centered on the viewport center.
    /// </summary>
    public void ZoomTo(double viewportZoom)
    {
        var zoomCenter = new Point(ContentOffsetX + (ContentViewportWidth / 2), ContentOffsetY + (ContentViewportHeight / 2));
        ZoomAboutPoint(viewportZoom, zoomCenter);
    }

    /// <summary>
    /// Do animation that scales the content so that it fits completely in the control.
    /// </summary>
    public void AnimatedScaleToFit()
    {
        if (_content == null)
            throw new ApplicationException($"{PART_Content} was not found in the ZoomAndPanControl visual template!");
        ZoomTo(FillZoomValue);
        //AnimatedZoomTo(new Rect(0, 0, _content.ActualWidth, _content.ActualHeight));
    }

    /// <summary>
    /// Instantly scale the content so that it fits completely in the control.
    /// </summary>
    public void ScaleToFit()
    {
        if (_content == null)
            throw new ApplicationException($"{PART_Content} was not found in the ZoomAndPanControl visual template!");
        ZoomTo(FitZoomValue);
        //ZoomTo(new Rect(0, 0, _content.ActualWidth, _content.ActualHeight));
    }

    /// <summary>
    /// Zoom to the specified scale and move the specified focus point to the center of the viewport.
    /// </summary>
    private void AnimatedZoomPointToViewportCenter(double newContentZoom, Point contentZoomFocus, EventHandler? callback)
    {
        newContentZoom = Math.Min(Math.Max(newContentZoom, MinimumZoomClamped), MaximumZoom);

        AnimationHelper.CancelAnimation(this, ContentZoomFocusXProperty);
        AnimationHelper.CancelAnimation(this, ContentZoomFocusYProperty);
        AnimationHelper.CancelAnimation(this, ViewportZoomFocusXProperty);
        AnimationHelper.CancelAnimation(this, ViewportZoomFocusYProperty);

        ContentZoomFocusX = contentZoomFocus.X;
        ContentZoomFocusY = contentZoomFocus.Y;
        ViewportZoomFocusX = (ContentZoomFocusX - ContentOffsetX) * InternalViewportZoom;
        ViewportZoomFocusY = (ContentZoomFocusY - ContentOffsetY) * InternalViewportZoom;

        //
        // When zooming about a point make updates to ViewportZoom also update content offset.
        //
        _enableContentOffsetUpdateFromScale = true;

        AnimationHelper.StartAnimation(this, InternalViewportZoomProperty, newContentZoom, AnimationDuration,
            delegate (object? sender, EventArgs e)
            {
                _enableContentOffsetUpdateFromScale = false;
                callback?.Invoke(this, EventArgs.Empty);
            }, UseAnimations);

        AnimationHelper.StartAnimation(this, ViewportZoomFocusXProperty, ViewportWidth / 2, AnimationDuration, UseAnimations);
        AnimationHelper.StartAnimation(this, ViewportZoomFocusYProperty, ViewportHeight / 2, AnimationDuration, UseAnimations);
    }

    /// <summary>
    /// Zoom to the specified scale and move the specified focus point to the center of the viewport.
    /// </summary>
    private void ZoomPointToViewportCenter(double newContentZoom, Point contentZoomFocus)
    {
        newContentZoom = Math.Min(Math.Max(newContentZoom, MinimumZoomClamped), MaximumZoom);

        AnimationHelper.CancelAnimation(this, InternalViewportZoomProperty);
        AnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
        AnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

        InternalViewportZoom = newContentZoom;
        ContentOffsetX = contentZoomFocus.X - (ContentViewportWidth / 2);
        ContentOffsetY = contentZoomFocus.Y - (ContentViewportHeight / 2);
    }

    /// <summary>
    /// Initialise the rectangle that the use is dragging out.
    /// </summary>
    private void InitDragZoomRect(Point pt1, Point pt2)
    {
        if (_partDragZoomCanvas is null || _partDragZoomBorder is null)
        {
            return;
        }

        _partDragZoomCanvas.Visibility = Visibility.Visible;
        _partDragZoomBorder.Opacity = 1;
        SetDragZoomRect(pt1, pt2);
    }

    /// <summary>
    /// Update the position and size of the rectangle that user is dragging out.
    /// </summary>
    private void SetDragZoomRect(Point pt1, Point pt2)
    {
        //
        // Update the coordinates of the rectangle that is being dragged out by the user.
        // The we offset and rescale to convert from content coordinates.
        //
        if (_partDragZoomCanvas is null || _partDragZoomBorder is null)
        {
            return;
        }

        var rect = ViewportHelpers.Clip(pt1, pt2, new Point(0, 0),
            new Point(_partDragZoomCanvas.ActualWidth, _partDragZoomCanvas.ActualHeight));
        ViewportHelpers.PositionBorderOnCanvas(_partDragZoomBorder, rect);
    }

    /// <summary>
    /// When the user has finished dragging out the rectangle the zoom operation is applied.
    /// </summary>
    private void ApplyDragZoomRect(Point finalContentMousePoint)
    {
        if (_partDragZoomCanvas is null || _partDragZoomBorder is null)
        {
            return;
        }

        var rect = ViewportHelpers.Clip(finalContentMousePoint, _origContentMouseDownPoint, new Point(0, 0),
            new Point(_partDragZoomCanvas.ActualWidth, _partDragZoomCanvas.ActualHeight));
        AnimatedZoomTo(rect);
        // new Rect(contentX, contentY, contentWidth, contentHeight));
        FadeOutDragZoomRect();
    }

    /// <summary>
    /// Fade out the drag zoom rectangle.
    /// </summary>
    private void FadeOutDragZoomRect()
    {
        if (_partDragZoomCanvas is null || _partDragZoomBorder is null)
        {
            return;
        }

        AnimationHelper.StartAnimation(
            _partDragZoomBorder,
            OpacityProperty,
            0.0,
            0.1,
            delegate { _partDragZoomCanvas.Visibility = Visibility.Collapsed; },
            UseAnimations);
    }

    /// <summary>
    /// When content is renewed, set the initial position as specified
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SetZoomAndPanInitialPosition(object sender, SizeChangedEventArgs e)
    {
        if (_content is null)
        {
            InternalViewportZoom = FitZoomValue;
            return;
        }

        switch (ZoomAndPanInitialPosition)
        {
            case ZoomAndPanInitialPositionType.Default:
                break;
            case ZoomAndPanInitialPositionType.FitScreen:
                InternalViewportZoom = FitZoomValue;
                break;
            case ZoomAndPanInitialPositionType.FillScreen:
                InternalViewportZoom = FillZoomValue;
                ContentOffsetX = (_content.ActualWidth - ViewportWidth / InternalViewportZoom) / 2;
                ContentOffsetY = (_content.ActualHeight - ViewportHeight / InternalViewportZoom) / 2;
                break;
            case ZoomAndPanInitialPositionType.OneHundredPercentCentered:
                InternalViewportZoom = 1.0;
                ContentOffsetX = (_content.ActualWidth - ViewportWidth) / 2;
                ContentOffsetY = (_content.ActualHeight - ViewportHeight) / 2;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Reset the viewport zoom focus to the center of the viewport.
    /// </summary>
    private void ResetViewportZoomFocus()
    {
        ViewportZoomFocusX = ViewportWidth / 2;
        ViewportZoomFocusY = ViewportHeight / 2;
    }

    /// <summary>
    /// Update the viewport size from the specified size.
    /// </summary>
    private void UpdateViewportSize(Size newSize)
    {
        if (_viewport == newSize)
            return;

        _viewport = newSize;

        //
        // Update the viewport size in content coordiates.
        //
        UpdateContentViewportSize();

        //
        // Initialise the content zoom focus point.
        //
        UpdateContentZoomFocusX();
        UpdateContentZoomFocusY();

        //
        // Reset the viewport zoom focus to the center of the viewport.
        //
        ResetViewportZoomFocus();

        //
        // Update content offset from itself when the size of the viewport changes.
        // This ensures that the content offset remains properly clamped to its valid range.
        //
        ContentOffsetX = ContentOffsetX;
        ContentOffsetY = ContentOffsetY;

        //
        // Tell that owning ScrollViewer that scrollbar data has changed.
        //
        ScrollOwner?.InvalidateScrollInfo();
    }

    /// <summary>
    /// Update the size of the viewport in content coordinates after the viewport size or 'ViewportZoom' has changed.
    /// </summary>
    private void UpdateContentViewportSize()
    {
        ContentViewportWidth = ViewportWidth / InternalViewportZoom;
        ContentViewportHeight = ViewportHeight / InternalViewportZoom;

        _constrainedContentViewportWidth = Math.Min(ContentViewportWidth, _unScaledExtent.Width);
        _constrainedContentViewportHeight = Math.Min(ContentViewportHeight, _unScaledExtent.Height);

        UpdateTranslationX();
        UpdateTranslationY();
    }

    /// <summary>
    /// Update the X coordinate of the translation transformation.
    /// </summary>
    private void UpdateTranslationX()
    {
        if (_contentOffsetTransform != null)
        {
            var scaledContentWidth = _unScaledExtent.Width * InternalViewportZoom;
            if (scaledContentWidth < ViewportWidth)
                //
                // When the content can fit entirely within the viewport, center it.
                //
                _contentOffsetTransform.X = (ContentViewportWidth - _unScaledExtent.Width) / 2;
            else
                _contentOffsetTransform.X = -ContentOffsetX;
        }
    }

    /// <summary>
    /// Update the Y coordinate of the translation transformation.
    /// </summary>
    private void UpdateTranslationY()
    {
        if (_contentOffsetTransform != null)
        {
            var scaledContentHeight = _unScaledExtent.Height * InternalViewportZoom;
            if (scaledContentHeight < ViewportHeight)
                //
                // When the content can fit entirely within the viewport, center it.
                //
                _contentOffsetTransform.Y = (ContentViewportHeight - _unScaledExtent.Height) / 2;
            else
                _contentOffsetTransform.Y = -ContentOffsetY;
        }
    }

    /// <summary>
    /// Update the X coordinate of the zoom focus point in content coordinates.
    /// </summary>
    private void UpdateContentZoomFocusX()
    {
        ContentZoomFocusX = ContentOffsetX + (_constrainedContentViewportWidth / 2);
    }

    /// <summary>
    /// Update the Y coordinate of the zoom focus point in content coordinates.
    /// </summary>
    private void UpdateContentZoomFocusY()
    {
        ContentZoomFocusY = ContentOffsetY + (_constrainedContentViewportHeight / 2);
    }

    private void SetCurrentZoomTypeEnum()
    {
        _currentZoomTypeEnum = ViewportZoom.IsWithinOnePercent(FitZoomValue)
            ? CurrentZoomType.Fit
            : ViewportZoom.IsWithinOnePercent(FillZoomValue) ? CurrentZoomType.Fill : CurrentZoomType.Other;
    }

    private void UpdateInternalViewportZoom()
    {
        InternalViewportZoom = Math.Min(Math.Max(InternalViewportZoom, MinimumZoomClamped), MaximumZoom);
    }

    /// <summary>
    /// Event raised when the 'ViewportZoom' property has changed value.
    /// </summary>
    private void OnInternalViewportZoomChanged(DependencyPropertyChangedEventArgs e)
    {
        if (_contentZoomTransform != null)
        {
            //
            // Update the content scale transform whenever 'ViewportZoom' changes.
            //
            _contentZoomTransform.ScaleX = InternalViewportZoom;
            _contentZoomTransform.ScaleY = InternalViewportZoom;
        }

        //
        // Update the size of the viewport in content coordinates.
        //
        UpdateContentViewportSize();

        if (_enableContentOffsetUpdateFromScale)
        {
            try
            {
                // 
                // Disable content focus syncronization.  We are about to update content offset whilst zooming
                // to ensure that the viewport is focused on our desired content focus point.  Setting this
                // to 'true' stops the automatic update of the content focus when content offset changes.
                //
                _disableContentFocusSync = true;

                //
                // Whilst zooming in or out keep the content offset up-to-date so that the viewport is always
                // focused on the content focus point (and also so that the content focus is locked to the 
                // viewport focus point - this is how the google maps style zooming works).
                //
                var viewportOffsetX = ViewportZoomFocusX - (ViewportWidth / 2);
                var viewportOffsetY = ViewportZoomFocusY - (ViewportHeight / 2);
                var contentOffsetX = viewportOffsetX / InternalViewportZoom;
                var contentOffsetY = viewportOffsetY / InternalViewportZoom;
                ContentOffsetX = (ContentZoomFocusX - (ContentViewportWidth / 2)) - contentOffsetX;
                ContentOffsetY = (ContentZoomFocusY - (ContentViewportHeight / 2)) - contentOffsetY;
            }
            finally
            {
                _disableContentFocusSync = false;
            }
        }
        ContentZoomChanged?.Invoke(this, EventArgs.Empty);
        ViewportZoom = InternalViewportZoom;
        OnPropertyChanged(new DependencyPropertyChangedEventArgs(ViewportZoomProperty, ViewportZoom, InternalViewportZoom));
        ScrollOwner?.InvalidateScrollInfo();
        SetCurrentZoomTypeEnum();
    }

    /// <summary>
    /// Method called to clamp the 'ViewportZoom' value to its valid range.
    /// </summary>
    private object OnInternalViewportZoomCoerce(object baseValue)
    {
        var value = Math.Max((double)baseValue, MinimumZoomClamped);
        value = MinimumZoomType switch
        {
            MinimumZoomType.FitScreen => Math.Min(Math.Max(value, FitZoomValue), MaximumZoom),
            MinimumZoomType.FillScreen => Math.Min(Math.Max(value, FillZoomValue), MaximumZoom),
            MinimumZoomType.MinimumZoom => Math.Min(Math.Max(value, MinimumZoom), MaximumZoom),
            _ => throw new ArgumentOutOfRangeException(),
        };
        return value;
    }

    private void OuterBackgroundChanged(DependencyPropertyChangedEventArgs e)
    {
        if (_croppingAdorner != null)
            _croppingAdorner.Fill = (Brush)e.NewValue;
    }

    /// <summary>
    /// Event raised when the 'ContentOffsetX' property has changed value.
    /// </summary>
    private void OnContentOffsetXChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateTranslationX();

        if (!_disableContentFocusSync)
            //
            // Normally want to automatically update content focus when content offset changes.
            // Although this is disabled using 'disableContentFocusSync' when content offset changes due to in-progress zooming.
            //
            UpdateContentZoomFocusX();
        //
        // Raise an event to let users of the control know that the content offset has changed.
        //
        ContentOffsetXChanged?.Invoke(this, EventArgs.Empty);

        if (!_disableScrollOffsetSync)
            //
            // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
            //
            ScrollOwner?.InvalidateScrollInfo();
    }

    /// <summary>
    /// Method called to clamp the 'ContentOffsetX' value to its valid range.
    /// </summary>
    private object OnContentOffsetXCoerce(object baseValue)
    {
        var value = (double)baseValue;
        var minOffsetX = 0.0;
        var maxOffsetX = Math.Max(0.0, _unScaledExtent.Width - _constrainedContentViewportWidth);
        value = Math.Min(Math.Max(value, minOffsetX), maxOffsetX);
        return value;
    }

    /// <summary>
    /// Event raised when the 'ContentOffsetY' property has changed value.
    /// </summary>
    private void OnContentOffsetYChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateTranslationY();

        if (!_disableContentFocusSync)
            //
            // Normally want to automatically update content focus when content offset changes.
            // Although this is disabled using 'disableContentFocusSync' when content offset changes due to in-progress zooming.
            //
            UpdateContentZoomFocusY();
        if (!_disableScrollOffsetSync)
            //
            // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
            //
            ScrollOwner?.InvalidateScrollInfo();
        //
        // Raise an event to let users of the control know that the content offset has changed.
        //
        ContentOffsetYChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Method called to clamp the 'ContentOffsetY' value to its valid range.
    /// </summary>
    private object OnContentOffsetYCoerce(object baseValue)
    {
        var value = (double)baseValue;
        var minOffsetY = 0.0;
        var maxOffsetY = Math.Max(0.0, _unScaledExtent.Height - _constrainedContentViewportHeight);
        value = Math.Min(Math.Max(value, minOffsetY), maxOffsetY);
        return value;
    }

    /// <summary>
    /// Fits the content within the viewport by zooming to the appropriate level.
    /// </summary>
    public void Fit()
    {
        if (CanFit)
        {
            SaveZoom();
            AnimatedZoomTo(FitZoomValue);
        }
    }

    /// <summary>
    /// Fills the viewport with content by zooming in, centered around the content.
    /// </summary>
    public void Fill()
    {
        if (CanFill)
        {
            SaveZoom();
            AnimatedZoomToCentered(FillZoomValue);
        }
    }

    /// <summary>
    /// Jump back to the most recent zoom level saved on redo stack.
    /// </summary>
    private void RedoZoom()
    {
        _viewportZoomCache = CreateUndoRedoStackItem();
        if (!_redoStack.Any() || !_viewportZoomCache.Equals(_redoStack.Peek()))
            _undoStack.Push(_viewportZoomCache);
        _viewportZoomCache = _redoStack.Pop();
        AnimatedZoomTo(_viewportZoomCache.Zoom, _viewportZoomCache.Rect);
        SetScrollViewerFocus();
    }

    /// <summary>
    /// Jump back to the previous zoom level, saving current zoom to Redo Stack.
    /// </summary>
    private void UndoZoom()
    {
        _viewportZoomCache = CreateUndoRedoStackItem();
        if (!_undoStack.Any() || !_viewportZoomCache.Equals(_undoStack.Peek()))
            _redoStack.Push(_viewportZoomCache);
        _viewportZoomCache = _undoStack.Pop();
        AnimatedZoomTo(_viewportZoomCache.Zoom, _viewportZoomCache.Rect);
        SetScrollViewerFocus();
    }

    /// <summary>
    /// Zoom the viewport out, centering on the specified point (in content coordinates).
    /// </summary>
    private void ZoomOut(Point contentZoomCenter)
    {
        if (CanZoomOut())
            ZoomAboutPoint(InternalViewportZoom * 0.90909090909, contentZoomCenter);
    }

    private bool CanZoomOut()
    {
        return InternalViewportZoom > MinimumZoomClamped;
    }

    /// <summary>
    /// Zoom the viewport in, centering on the specified point (in content coordinates).
    /// </summary>
    private void ZoomIn(Point contentZoomCenter)
    {
        if (CanZoomIn()) ZoomAboutPoint(InternalViewportZoom * 1.1, contentZoomCenter);
    }

    /// <summary>
    /// Determines if the viewport can be zoomed out based on the current zoom level.
    /// </summary>
    /// <returns><c>true</c> if zooming out is possible; otherwise, <c>false</c>.</returns>
    private bool CanZoomIn()
    {
        return InternalViewportZoom < MaximumZoom;
    }

    /// <summary>
    /// Zooms the viewport by a specified percentage.
    /// </summary>
    /// <param name="percent">The percentage to zoom, where 100 represents the current zoom level.</param>
    public void ZoomPercent(double percent)
    {
        if (CanZoomPercent(percent))
        {
            var adjustedValue = Convert.ToDouble(percent) == 0 ? 1 : Convert.ToDouble(percent) / 100;
            SaveZoom();
            AnimatedZoomTo(adjustedValue);
        }
    }

    /// <summary>
    /// Determines if the viewport can be zoomed to a specified percentage.
    /// </summary>
    /// <param name="percent">The percentage to zoom.</param>
    /// <returns><c>true</c> if zooming to the percentage is possible; otherwise, <c>false</c>.</returns>
    private bool CanZoomPercent(double percent)
    {
        var adjustedValue = Convert.ToDouble(percent) == 0 ? 1 : Convert.ToDouble(percent) / 100;
        return !InternalViewportZoom.IsWithinOnePercent(adjustedValue) && adjustedValue >= MinimumZoomClamped;
    }

    /// <summary>
    /// Zooms the viewport from the minimum zoom level based on a specified ratio.
    /// </summary>
    /// <param name="ratio">The ratio to zoom from the minimum zoom level.</param>
    public void ZoomRatioFromMinimum(int ratio)
    {
        if (CanZoomRatioFromMinimum(ratio))
        {
            var adjustedValue = (Convert.ToDouble(ratio) == 0 ? 2 : Convert.ToDouble(ratio)) * MinimumZoomClamped;
            SaveZoom();
            AnimatedZoomTo(adjustedValue);
        }
    }

    /// <summary>
    /// Determines if the viewport can be zoomed based on a specified ratio from the minimum zoom level.
    /// </summary>
    /// <param name="ratio">The ratio to check against the minimum zoom level.</param>
    /// <returns><c>true</c> if zooming based on the ratio is possible; otherwise, <c>false</c>.</returns>
    private bool CanZoomRatioFromMinimum(int ratio)
    {
        var adjustedValue = (Convert.ToDouble(ratio) == 0 ? 2 : Convert.ToDouble(ratio)) * MinimumZoomClamped;
        return !InternalViewportZoom.IsWithinOnePercent(adjustedValue) && adjustedValue >= MinimumZoomClamped;
    }

    /// <summary>
    /// Event raised 'MinimumZoom' has changed.
    /// </summary>
    private void OnMinimumZoomChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateInternalViewportZoom();
    }

    /// <summary>
    /// Event raised 'MinimumZoom' or 'MaximumZoom' has changed.
    /// </summary>
    private void OnMaximumZoomChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateInternalViewportZoom();
    }

    /// <summary>
    /// Event raised 'MinimumZoom' or 'MaximumZoom' has changed.
    /// </summary>
    private void OnMousePositionChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateInternalViewportZoom();
    }

    /// <summary>
    /// Handles the changes to the ViewportZoom dependency property.
    /// </summary>
    /// <param name="e">The event data containing the details of the property change.</param>
    private void OnViewportZoomChanged(DependencyPropertyChangedEventArgs e)
    {
        var newZoom = (double)e.NewValue;
        if (InternalViewportZoom != newZoom)
        {
            var centerPoint = new Point(ContentOffsetX + (_constrainedContentViewportWidth / 2), ContentOffsetY + (_constrainedContentViewportHeight / 2));
            ZoomAboutPoint(newZoom, centerPoint);
        }

        SetClippingRectangle();

        ViewportZoomChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Sets the clipping rectangle for the cropping adorner based on content offsets and zoom level.
    /// </summary>
    private void SetClippingRectangle()
    {
        if (_croppingAdorner is null)
        {
            return;
        }

        if (ContentOffsetX > 0 && ContentOffsetY > 0)
        {
            _croppingAdorner.ClippingRectangle = new Rect(0, 0, ActualWidth, ActualHeight);
        }
        else if (ContentOffsetY > 0)
        {
            _croppingAdorner.ClippingRectangle = new Rect(
                ContentViewportWidth * ViewportZoom / 2 - ExtentWidth / 2,
                0,
                ExtentWidth,
                ActualHeight);
        }
        else if (ContentOffsetX > 0)
        {
            _croppingAdorner.ClippingRectangle = new Rect(
                0,
                ContentViewportHeight * ViewportZoom / 2 - ExtentHeight / 2,
                ActualWidth,
                ExtentHeight);
        }
        else
        {
            if (_croppingAdorner != null)
            {
                _croppingAdorner.ClippingRectangle = new Rect(
                    ExtentWidth > 0 ? ContentViewportWidth * ViewportZoom / 2 - ExtentWidth / 2 : 0,
                    ExtentHeight > 0 ? ContentViewportHeight * ViewportZoom / 2 - ExtentHeight / 2 : 0,
                    ExtentWidth,
                    ExtentHeight);
            }
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Executes the Fit command, adjusting the viewport to fit the content.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the executed command.</param>
    private static void OnFitExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var zoomAndPanControl = (ZoomAndPanControl)sender;
        zoomAndPanControl.Fit();
    }

    /// <summary>
    /// Determines if the Fit command can be executed.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the command execution status.</param>
    private static void OnFitCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is ZoomAndPanControl zoomAndPanControl && zoomAndPanControl.CanFit;
    }

    /// <summary>
    /// Executes the Fill command, adjusting the viewport to fill the content.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the executed command.</param>
    private static void OnFillExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var zoomAndPanControl = (ZoomAndPanControl)sender;
        zoomAndPanControl.Fill();
    }

    /// <summary>
    /// Determines if the Fill command can be executed.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the command execution status.</param>
    private static void OnFillCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is ZoomAndPanControl zoomAndPanControl && zoomAndPanControl.CanFill;
    }

    /// <summary>
    /// Executes the RedoZoom command, restoring the most recent zoom level from the redo stack.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the executed command.</param>
    private static void OnRedoZoomExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var zoomAndPanControl = (ZoomAndPanControl)sender;
        zoomAndPanControl.RedoZoom();
    }

    /// <summary>
    /// Determines if the RedoZoom command can be executed.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the command execution status.</param>
    private static void OnRedoZoomCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is ZoomAndPanControl zoomAndPanControl && zoomAndPanControl.CanRedoZoom;
    }

    /// <summary>
    /// Executes the UndoZoom command, reverting to the previous zoom level.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the executed command.</param>
    private static void OnUndoZoomExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var zoomAndPanControl = (ZoomAndPanControl)sender;
        zoomAndPanControl.UndoZoom();
    }

    /// <summary>
    /// Determines if the UndoZoom command can be executed.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the command execution status.</param>
    private static void OnUndoZoomCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is ZoomAndPanControl zoomAndPanControl && zoomAndPanControl.CanUndoZoom;
    }

    /// <summary>
    /// Executes the ZoomOut command, zooming out centered on the current content zoom focus.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the executed command.</param>
    private static void OnZoomOutExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var zoomAndPanControl = (ZoomAndPanControl)sender;
        zoomAndPanControl.DelayedSaveZoom1500Miliseconds();
        zoomAndPanControl.ZoomOut(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
    }

    /// <summary>
    /// Determines if the ZoomOut command can be executed.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the command execution status.</param>
    private static void OnZoomOutCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is ZoomAndPanControl zoomAndPanControl && zoomAndPanControl.CanZoomOut();
    }

    /// <summary>
    /// Executes the ZoomIn command, zooming in centered on the current content zoom focus.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the executed command.</param>
    private static void OnZoomInExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var zoomAndPanControl = (ZoomAndPanControl)sender;
        zoomAndPanControl.DelayedSaveZoom1500Miliseconds();
        zoomAndPanControl.ZoomIn(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
    }

    /// <summary>
    /// Determines if the ZoomIn command can be executed.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the command execution status.</param>
    private static void OnZoomInCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is ZoomAndPanControl zoomAndPanControl && zoomAndPanControl.CanZoomIn();
    }

    /// <summary>
    /// Executes the ZoomPercent command, adjusting the zoom level by a specified percentage.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the executed command.</param>
    private static void OnZoomPercentExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var zoomAndPanControl = (ZoomAndPanControl)sender;
        zoomAndPanControl.ZoomPercent((double)e.Parameter);
    }

    /// <summary>
    /// Determines if the ZoomPercent command can be executed.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the command execution status.</param>
    private static void OnZoomPercentCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is ZoomAndPanControl zoomAndPanControl && e.Parameter is double value && zoomAndPanControl.CanZoomPercent(value);
    }

    /// <summary>
    /// Executes the ZoomRatioFromMinimum command, adjusting the zoom level based on a specified ratio from the minimum zoom.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the executed command.</param>
    private static void OnZoomRatioFromMinimumExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var zoomAndPanControl = (ZoomAndPanControl)sender;
        zoomAndPanControl.ZoomRatioFromMinimum((int)e.Parameter);
    }

    /// <summary>
    /// Determines if the ZoomRatioFromMinimum command can be executed.
    /// </summary>
    /// <param name="sender">The source of the command.</param>
    /// <param name="e">The event data for the command execution status.</param>
    private static void OnZoomRatioFromMinimumCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is ZoomAndPanControl zoomAndPanControl && e.Parameter is int value && zoomAndPanControl.CanZoomRatioFromMinimum(value);
    }

    #endregion
}
