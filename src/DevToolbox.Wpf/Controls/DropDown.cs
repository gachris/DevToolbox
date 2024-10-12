using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using DevToolbox.Wpf.Automation.Peers;
using DevToolbox.Wpf.Interop;
using DevToolbox.Wpf.Utils;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a customizable dropdown control that can display content in a popup.
/// </summary>
/// <remarks>
/// The <see cref="DropDown"/> control allows users to interact with a toggle button that 
/// displays a popup containing additional content. The control can be resized and supports 
/// commands for integration with command bindings in WPF.
/// </remarks>
[TemplatePart(Name = PART_ToggleButton, Type = typeof(ToggleButton))]
[TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
[TemplatePart(Name = PART_PopupContent, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_ResizeThumb, Type = typeof(Thumb))]
[TemplatePart(Name = PART_ContentPresenter, Type = typeof(ContentPresenter))]
public partial class DropDown : HeaderedContentControl, ICommandSource
{
    #region Fields/Consts

    private const double MinSize = 0;

    /// <summary> 
    /// Identifies the toggle button part of the DropDown.
    /// </summary>
    protected const string PART_ToggleButton = "PART_ToggleButton";

    /// <summary> 
    /// Identifies the popup part of the DropDown.
    /// </summary>
    protected const string PART_Popup = "PART_Popup";

    /// <summary> 
    /// Identifies the popup content part of the DropDown.
    /// </summary>
    protected const string PART_PopupContent = "PART_PopupContent";

    /// <summary> 
    /// Identifies the resize thumb part of the DropDown.
    /// </summary>
    protected const string PART_ResizeThumb = "PART_ResizeThumb";

    /// <summary> 
    /// Identifies the content presenter part of the DropDown.
    /// </summary>
    protected const string PART_ContentPresenter = "PART_ContentPresenter";

    private static readonly MethodInfo? RepositionMethodInfo = typeof(Popup).GetMethod("Reposition", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly MethodInfo? ClearDropOppositeMethodInfo = typeof(Popup).GetMethod("ClearDropOpposite", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly PropertyInfo? DropOppositePropertyInfo = typeof(Popup).GetProperty("DropOpposite", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly PropertyInfo? PlacementInternalPropertyInfo = typeof(Popup).GetProperty("PlacementInternal", BindingFlags.NonPublic | BindingFlags.Instance);

    private ButtonBase? _button;
    private Popup? _popup;
    private FrameworkElement? _popupContent;
    private Thumb? _thumb;
    private Window? _window;
    private ContentPresenter? _contentPresenter;

    private EventHandler? _canExecuteChangedHandler;

    /// <summary>
    /// Identifies the <see cref="DropDown.PlacementRectangleProperty"/> 
    /// dependency property. </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.PlacementRectangleProperty"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty PlacementRectangleProperty =
        Popup.PlacementRectangleProperty.AddOwner(typeof(DropDown));

    /// <summary>
    /// Identifies the <see cref="DropDown.IsOpen"/> dependency property. 
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.IsOpen"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty IsOpenProperty =
        Popup.IsOpenProperty.AddOwner(typeof(DropDown), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

    /// <summary> 
    /// Identifies the <see cref="DropDown.AllowResize"/> dependency property. 
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.AllowResize"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty AllowResizeProperty =
        DependencyProperty.Register(nameof(AllowResize), typeof(bool), typeof(DropDown), new UIPropertyMetadata(false));

    /// <summary>
    /// Identifies the <see cref="DropDown.VerticalOffset"/> dependency property.
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.VerticalOffset"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty VerticalOffsetProperty =
        Popup.VerticalOffsetProperty.AddOwner(typeof(DropDown));

    /// <summary> 
    /// Identifies the <see cref="DropDown.HorizontalOffset"/> dependency property. 
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.HorizontalOffset"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty HorizontalOffsetProperty =
        Popup.HorizontalOffsetProperty.AddOwner(typeof(DropDown));

    /// <summary>
    /// Identifies the <see cref="DropDown.PopupAnimation"/> dependency property.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.PopupAnimation"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty PopupAnimationProperty =
        Popup.PopupAnimationProperty.AddOwner(typeof(DropDown));

    /// <summary> 
    /// Identifies the <see cref="DropDown.Placement"/> dependency property. 
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.Placement"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty PlacementProperty =
        Popup.PlacementProperty.AddOwner(typeof(DropDown));

    /// <summary>
    /// Identifies the <see cref="DropDown.CustomPopupPlacementCallback"/> dependency property.
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.CustomPopupPlacementCallback"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty CustomPopupPlacementCallbackProperty =
        Popup.CustomPopupPlacementCallbackProperty.AddOwner(typeof(DropDown));

    /// <summary> 
    /// Identifies the <see cref="DropDown.PlacementTarget"/> dependency property. 
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.PlacementTarget"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty PlacementTargetProperty =
        Popup.PlacementTargetProperty.AddOwner(typeof(DropDown));

    /// <summary>
    /// Identifies the <see cref="DropDown.StaysOpen"/> dependency property.
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.StaysOpen"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty StaysOpenProperty =
        Popup.StaysOpenProperty.AddOwner(typeof(DropDown), new PropertyMetadata(default(bool), OnStaysOpenChanged));

    /// <summary>
    /// Identifies the <see cref="DropDown.MinDropDownWidth"/> dependency property. 
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.MinDropDownWidth"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty MinDropDownWidthProperty =
        DependencyProperty.Register(nameof(MinDropDownWidth), typeof(double), typeof(DropDown),
            new FrameworkPropertyMetadata(MinSize, FrameworkPropertyMetadataOptions.AffectsMeasure, null, (d, value) => (double)value >= MinSize ? value : MinSize), new ValidateValueCallback(IsMinWidthHeightValid));

    /// <summary> 
    /// Identifies the <see cref="DropDown.DropDownWidth"/> dependency property. 
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.DropDownWidth"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty DropDownWidthProperty =
        DependencyProperty.Register(nameof(DropDownWidth), typeof(double), typeof(DropDown), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(IsWidthHeightValid));

    /// <summary> 
    /// Identifies the <see cref="DropDown.MaxDropDownWidth"/> dependency property.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.MaxDropDownWidth"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty MaxDropDownWidthProperty =
        DependencyProperty.Register(nameof(MaxDropDownWidth), typeof(double), typeof(DropDown), new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(IsMaxWidthHeightValid));

    /// <summary>
    /// Identifies the <see cref="DropDown.MinDropDownHeight"/> dependency property.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.MinDropDownHeight"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty MinDropDownHeightProperty =
        DependencyProperty.Register(nameof(MinDropDownHeight), typeof(double), typeof(DropDown),
            new FrameworkPropertyMetadata(MinSize, FrameworkPropertyMetadataOptions.AffectsMeasure, null, (d, value) => (double)value >= MinSize ? value : MinSize), new ValidateValueCallback(IsMinWidthHeightValid));

    /// <summary> 
    /// Identifies the <see cref="DropDown.DropDownHeight"/> dependency property. 
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.DropDownHeight"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty DropDownHeightProperty =
        DependencyProperty.Register(nameof(DropDownHeight), typeof(double), typeof(DropDown),
            new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(IsWidthHeightValid));

    /// <summary> 
    /// Identifies the <see cref="DropDown.MaxDropDownHeight"/> dependency property. 
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.MaxDropDownHeight"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(DropDown), new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMaxDropDownHeightChanged), new ValidateValueCallback(IsMaxWidthHeightValid));

    /// <summary>
    /// Identifies the <see cref="DropDown.DropDownCornerRadius"/> dependency property.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.DropDownCornerRadius"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty DropDownCornerRadiusProperty =
        DependencyProperty.Register(nameof(DropDownCornerRadius), typeof(CornerRadius), typeof(DropDown), new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary> 
    /// Identifies the <see cref="DropDown.SpeechBalloonPlacement"/> dependency property. 
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.SpeechBalloonPlacement"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty SpeechBalloonPlacementProperty =
        DependencyProperty.Register(nameof(SpeechBalloonPlacement), typeof(SpeechBalloonPlacement), typeof(DropDown),
            new FrameworkPropertyMetadata(SpeechBalloonPlacement.Auto, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary> 
    /// Identifies the <see cref="DropDown.SpeechBalloonHorizontalOffset"/> dependency property.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.SpeechBalloonHorizontalOffset"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty SpeechBalloonHorizontalOffsetProperty =
        DependencyProperty.Register(nameof(SpeechBalloonHorizontalOffset), typeof(double), typeof(DropDown), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Identifies the <see cref="DropDown.SpeechBalloonVerticalOffset"/> dependency property. 
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.SpeechBalloonVerticalOffset"/> dependency property. 
    /// </returns>
    public static readonly DependencyProperty SpeechBalloonVerticalOffsetProperty =
        DependencyProperty.Register(nameof(SpeechBalloonVerticalOffset), typeof(double), typeof(DropDown), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary> 
    /// Identifies the <see cref="DropDown.DropOppositePropertyKey"/> dependency property key.
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.DropOppositePropertyKey"/> dependency property key. 
    /// </returns>
    private static readonly DependencyPropertyKey DropOppositePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DropOpposite), typeof(bool), typeof(DropDown), new FrameworkPropertyMetadata(false));

    /// <summary> 
    /// Identifies the <see cref="DropDown.DropOpposite"/> dependency property.
    /// </summary>
    /// <returns> 
    /// The identifier for the <see cref="DropDown.DropOpposite"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty DropOppositeProperty = DropOppositePropertyKey.DependencyProperty;
    /// <summary>
    /// Identifies the <see cref="DropDown.IsDefault"/> dependency property.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.IsDefault"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty IsDefaultProperty =
        DependencyProperty.Register("IsDefault", typeof(bool), typeof(DropDown),
        new UIPropertyMetadata(false, OnIsDefaultChanged));

    /// <summary>
    /// Identifies the <see cref="DropDown.Command"/> dependency property.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.Command"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command", typeof(ICommand), typeof(DropDown),
        new PropertyMetadata(null, OnCommandChanged));

    /// <summary>
    /// Identifies the <see cref="DropDown.CommandParameter"/> dependency property.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.CommandParameter"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register("CommandParameter", typeof(object), typeof(DropDown),
        new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="DropDown.CommandTarget"/> dependency property.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.CommandTarget"/> dependency property.
    /// </returns>
    public static readonly DependencyProperty CommandTargetProperty =
        DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(DropDown),
        new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="DropDown.Click"/> routed event.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.Click"/> routed event.
    /// </returns>
    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DropDown));

    /// <summary>
    /// Identifies the <see cref="DropDown.Opened"/> routed event.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.Opened"/> routed event.
    /// </returns>
    public static readonly RoutedEvent OpenedEvent =
        EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DropDown));

    /// <summary>
    /// Identifies the <see cref="DropDown.Closed"/> routed event.
    /// </summary>
    /// <returns>
    /// The identifier for the <see cref="DropDown.Closed"/> routed event.
    /// </returns>
    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DropDown));

    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the button associated with the <see cref="DropDown"/> control.
    /// </summary>
    /// <returns>
    /// The <see cref="ButtonBase"/> associated with the <see cref="DropDown"/> control.
    /// </returns>
    protected ButtonBase? Button
    {
        get => _button;
        set
        {
            if (_button != null)
                _button.Click -= DropDownButton_Click;

            _button = value;

            if (_button != null)
                _button.Click += DropDownButton_Click;
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="DropDown"/> control is open.
    /// </summary>
    /// <returns>
    /// true if the <see cref="DropDown"/> is open; otherwise, false.
    /// </returns>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation of the <see cref="DropDown"/> control when it opens.
    /// </summary>
    /// <returns>
    /// A <see cref="PlacementMode"/> enumeration value that determines the orientation
    /// of the <see cref="DropDown"/> control when it opens. The default is <see cref="PlacementMode.Bottom"/>.
    /// </returns>
    [Bindable(true)]
    [Category("Layout")]
    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets a delegate handler method that positions the <see cref="DropDown"/> control.
    /// </summary>
    /// <returns>
    /// The <see cref="System.Windows.Controls.Primitives.CustomPopupPlacementCallback"/> delegate method
    /// that provides placement information for the <see cref="DropDown"/> control. The default is null.
    /// </returns>
    [Bindable(false)]
    [Category("Layout")]
    public CustomPopupPlacementCallback CustomPopupPlacementCallback
    {
        get => (CustomPopupPlacementCallback)GetValue(CustomPopupPlacementCallbackProperty);
        set => SetValue(CustomPopupPlacementCallbackProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="DropDown"/> control closes 
    /// when it is no longer in focus.
    /// </summary>
    /// <returns>
    /// true if the <see cref="DropDown"/> control closes when <see cref="DropDown.StaysOpen"/> 
    /// is set to false; otherwise, false. The default is true.
    /// </returns>
    [Bindable(true)]
    [Category("Behavior")]
    public bool StaysOpen
    {
        get => (bool)GetValue(StaysOpenProperty);
        set => SetValue(StaysOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal distance between the target origin and the popup alignment point.
    /// </summary>
    /// <returns>
    /// The horizontal distance between the target origin and the popup alignment point.
    /// The default is 0.
    /// </returns>
    [Bindable(true)]
    [Category("Layout")]
    [TypeConverter(typeof(LengthConverter))]
    public double HorizontalOffset
    {
        get => (double)GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical distance between the target origin and the popup alignment point.
    /// </summary>
    /// <returns>
    /// The vertical distance between the target origin and the popup alignment point.
    /// The default is 0.
    /// </returns>
    [Bindable(true)]
    [Category("Layout")]
    [TypeConverter(typeof(LengthConverter))]
    public double VerticalOffset
    {
        get => (double)GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the element relative to which the <see cref="DropDown"/> is positioned when it opens.
    /// </summary>
    /// <returns>
    /// The <see cref="System.Windows.UIElement"/> that is the logical parent of the <see cref="DropDown"/> control.
    /// The default is null.
    /// </returns>
    [Bindable(true)]
    [Category("Layout")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public UIElement PlacementTarget
    {
        get => (UIElement)GetValue(PlacementTargetProperty);
        set => SetValue(PlacementTargetProperty, value);
    }

    /// <summary>
    /// Gets or sets the rectangle relative to which the <see cref="DropDown"/> control is positioned when it opens.
    /// </summary>
    /// <returns>
    /// The rectangle that is used to position the <see cref="DropDown"/> control. The default is null.
    /// </returns>
    [Bindable(true)]
    [Category("Layout")]
    public Rect PlacementRectangle
    {
        get => (Rect)GetValue(PlacementRectangleProperty);
        set => SetValue(PlacementRectangleProperty, value);
    }

    /// <summary>
    /// Gets or sets an animation for the opening and closing of a <see cref="DropDown"/> control.
    /// </summary>
    /// <returns>
    /// The <see cref="System.Windows.Controls.Primitives.PopupAnimation"/> enumeration value that defines
    /// an animation to open and close a <see cref="DropDown"/> control. The default is <see cref="PopupAnimation.None"/>.
    /// </returns>
    [Bindable(true)]
    [Category("Appearance")]
    public PopupAnimation PopupAnimation
    {
        get => (PopupAnimation)GetValue(PopupAnimationProperty);
        set => SetValue(PopupAnimationProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether resizing is allowed for the <see cref="DropDown"/>.
    /// </summary>
    /// <returns>
    /// true if resizing is allowed; otherwise, false.
    /// </returns>
    public bool AllowResize
    {
        get => (bool)GetValue(AllowResizeProperty);
        set => SetValue(AllowResizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum width of the <see cref="DropDown"/>.
    /// </summary>
    /// <returns>
    /// The minimum width of the <see cref="DropDown"/>. The default is 0.
    /// </returns>
    public double MinDropDownWidth
    {
        get => (double)GetValue(MinDropDownWidthProperty);
        set => SetValue(MinDropDownWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the <see cref="DropDown"/>.
    /// </summary>
    /// <returns>
    /// The width of the <see cref="DropDown"/>. The default is NaN.
    /// </returns>
    public double DropDownWidth
    {
        get => (double)GetValue(DropDownWidthProperty);
        set => SetValue(DropDownWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum width of the <see cref="DropDown"/>.
    /// </summary>
    /// <returns>
    /// The maximum width of the <see cref="DropDown"/>. The default is double.PositiveInfinity.
    /// </returns>
    public double MaxDropDownWidth
    {
        get => (double)GetValue(MaxDropDownWidthProperty);
        set => SetValue(MaxDropDownWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum height of the <see cref="DropDown"/>.
    /// </summary>
    /// <returns>
    /// The minimum height of the <see cref="DropDown"/>. The default is 0.
    /// </returns>
    public double MinDropDownHeight
    {
        get => (double)GetValue(MinDropDownHeightProperty);
        set => SetValue(MinDropDownHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the <see cref="DropDown"/>.
    /// </summary>
    /// <returns>
    /// The height of the <see cref="DropDown"/>. The default is NaN.
    /// </returns>
    public double DropDownHeight
    {
        get => (double)GetValue(DropDownHeightProperty);
        set => SetValue(DropDownHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum height of the <see cref="DropDown"/>.
    /// </summary>
    /// <returns>
    /// The maximum height of the <see cref="DropDown"/>. The default is double.PositiveInfinity.
    /// </returns>
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius of the <see cref="DropDown"/> control.
    /// </summary>
    /// <returns>
    /// The <see cref="CornerRadius"/> for the <see cref="DropDown"/>.
    /// </returns>
    public CornerRadius DropDownCornerRadius
    {
        get => (CornerRadius)GetValue(DropDownCornerRadiusProperty);
        set => SetValue(DropDownCornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the placement of the speech balloon associated with the <see cref="DropDown"/> control.
    /// </summary>
    /// <returns>
    /// A value of <see cref="SpeechBalloonPlacement"/> that indicates where the speech balloon is positioned.
    /// The default is <see cref="SpeechBalloonPlacement.Bottom"/>.
    /// </returns>
    public SpeechBalloonPlacement SpeechBalloonPlacement
    {
        get => (SpeechBalloonPlacement)GetValue(SpeechBalloonPlacementProperty);
        set => SetValue(SpeechBalloonPlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal offset of the speech balloon.
    /// </summary>
    /// <returns>
    /// The horizontal distance from the speech balloon to the <see cref="DropDown"/> control.
    /// The default is 0.
    /// </returns>
    public double SpeechBalloonHorizontalOffset
    {
        get => (double)GetValue(SpeechBalloonHorizontalOffsetProperty);
        set => SetValue(SpeechBalloonHorizontalOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical offset of the speech balloon.
    /// </summary>
    /// <returns>
    /// The vertical distance from the speech balloon to the <see cref="DropDown"/> control.
    /// The default is 0.
    /// </returns>
    public double SpeechBalloonVerticalOffset
    {
        get => (double)GetValue(SpeechBalloonVerticalOffsetProperty);
        set => SetValue(SpeechBalloonVerticalOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="DropDown"/> should drop opposite to the specified 
    /// <see cref="Placement"/> property.
    /// </summary>
    /// <returns>
    /// true if the <see cref="DropDown"/> drops opposite; otherwise, false.
    /// </returns>
    public bool DropOpposite
    {
        get => (bool)GetValue(DropOppositeProperty);
        private set => SetValue(DropOppositePropertyKey, value);
    }

    /// <summary>
    /// Gets or sets the command associated with the <see cref="DropDown"/> control.
    /// </summary>
    /// <returns>
    /// The <see cref="ICommand"/> associated with the <see cref="DropDown"/>. The default is null.
    /// </returns>
    [TypeConverter(typeof(CommandConverter))]
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for the command associated with the <see cref="DropDown"/> control.
    /// </summary>
    /// <returns>
    /// The command parameter, which is of type <see cref="object"/>. The default is null.
    /// </returns>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the target element for the command associated with the <see cref="DropDown"/> control.
    /// </summary>
    /// <returns>
    /// The target element, which is of type <see cref="IInputElement"/>. The default is null.
    /// </returns>
    public IInputElement CommandTarget
    {
        get => (IInputElement)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="DropDown"/> is the default control.
    /// </summary>
    /// <returns>
    /// true if the <see cref="DropDown"/> is the default control; otherwise, false.
    /// </returns>
    public bool IsDefault
    {
        get => (bool)GetValue(IsDefaultProperty);
        set => SetValue(IsDefaultProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the <see cref="DropDown"/> control is clicked.
    /// </summary>
    /// <remarks>
    /// This event can be used to perform actions when the <see cref="DropDown"/> is activated by a click.
    /// </remarks>
    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    /// <summary>
    /// Occurs when the <see cref="DropDown"/> control is opened.
    /// </summary>
    /// <remarks>
    /// This event can be used to perform actions when the <see cref="DropDown"/> is opened,
    /// such as populating the control with data or changing the appearance.
    /// </remarks>
    public event RoutedEventHandler Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    /// <summary>
    /// Occurs when the <see cref="DropDown"/> control is closed.
    /// </summary>
    /// <remarks>
    /// This event can be used to perform actions when the <see cref="DropDown"/> is closed,
    /// such as saving state or resetting values.
    /// </remarks>
    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    #endregion

    static DropDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDown), new FrameworkPropertyMetadata(typeof(DropDown)));

        EventManager.RegisterClassHandler(typeof(DropDown), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DropDown"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor sets up the necessary event handlers for keyboard and mouse interactions.
    /// It listens for key down events and mouse down events occurring outside of the captured element,
    /// enabling appropriate behavior for the dropdown control.
    /// </remarks>
    public DropDown()
    {
        Keyboard.AddKeyDownHandler(this, OnKeyDown);
        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
    }

    #region Methods Override

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new DropDownAutomationPeer(this);
    }

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        Button = GetTemplateChild(PART_ToggleButton) as ToggleButton;

        _contentPresenter = GetTemplateChild(PART_ContentPresenter) as ContentPresenter;

        if (_popup != null)
            _popup.Opened -= Popup_Opened;

        _popup = GetTemplateChild(PART_Popup) as Popup;

        if (_popup != null)
            _popup.Opened += Popup_Opened;

        _popupContent = Template.FindName(PART_PopupContent, this) as FrameworkElement;

        if (_thumb != null)
            _thumb.DragDelta -= OnThumbDragDelta;

        _thumb = Template.FindName(PART_ResizeThumb, this) as Thumb;

        if (_thumb != null)
            _thumb.DragDelta += OnThumbDragDelta;

        if (_window != null)
        {
            _window.SizeChanged -= OnWindowLocationChanged;
            _window.LocationChanged -= OnWindowLocationChanged;
        }

        _window = Window.GetWindow(this);

        if (_window != null)
        {
            _window.SizeChanged += OnWindowLocationChanged;
            _window.LocationChanged += OnWindowLocationChanged;
        }
    }

    /// <inheritdoc/>
    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusWithinChanged(e);

        if (_popup is null || StaysOpen) return;

        if (!(bool)e.NewValue)
        {
            var contextMenu = GetContextMenu(_popup.Child);

            if (contextMenu == null)
                CloseDropDown(false);
            else
            {
                RoutedEventHandler? handler = null;
                handler = new RoutedEventHandler((s, a) =>
                {
                    contextMenu.Closed -= handler;
                    if (!IsKeyboardFocusWithin)
                        CloseDropDown(false);
                });
                contextMenu.Closed += handler;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);
        Button?.Focus();
    }

    /// <inheritdoc/>
    protected override void OnAccessKey(AccessKeyEventArgs e)
    {
        if (e.IsMultiple)
        {
            base.OnAccessKey(e);
        }
        else
        {
            OnClick();
        }
    }

    #endregion

    #region Methods
    /// <summary>
    /// Called when the value of the <see cref="IsDefault"/> property changes.
    /// </summary>
    /// <param name="oldValue">The old value of the <see cref="IsDefault"/> property.</param>
    /// <param name="newValue">The new value of the <see cref="IsDefault"/> property.</param>
    protected virtual void OnIsDefaultChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            // Register the access key for the Enter key (represented by "\r")
            AccessKeyManager.Register("\r", this);
        }
        else
        {
            // Unregister the access key when it is no longer the default
            AccessKeyManager.Unregister("\r", this);
        }
    }

    /// <summary>
    /// Called when the value of the <see cref="IsOpen"/> property changes.
    /// Raises the appropriate routed events when the dropdown is opened or closed.
    /// </summary>
    /// <param name="oldValue">The old value of the <see cref="IsOpen"/> property.</param>
    /// <param name="newValue">The new value of the <see cref="IsOpen"/> property.</param>
    protected virtual void OnIsOpenChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            // Raise the Opened event when the dropdown opens
            RaiseRoutedEvent(DropDown.OpenedEvent);
        }
        else
        {
            // Raise the Closed event when the dropdown closes
            RaiseRoutedEvent(DropDown.ClosedEvent);
        }
    }

    /// <summary>
    /// Called when the value of the <see cref="StaysOpen"/> property changes.
    /// Override this method to provide custom handling when the property changes.
    /// </summary>
    /// <param name="oldValue">The old value of the <see cref="StaysOpen"/> property.</param>
    /// <param name="newValue">The new value of the <see cref="StaysOpen"/> property.</param>
    protected virtual void OnStaysOpenChanged(bool oldValue, bool newValue)
    {
        // Custom handling for StaysOpen changes can be added here
    }

    /// <summary>
    /// Called when the value of the <see cref="MaxDropDownHeight"/> property changes.
    /// Override this method to provide custom handling when the property changes.
    /// </summary>
    /// <param name="oldValue">The old value of the <see cref="MaxDropDownHeight"/> property.</param>
    /// <param name="newValue">The new value of the <see cref="MaxDropDownHeight"/> property.</param>
    protected virtual void OnMaxDropDownHeightChanged(double oldValue, double newValue)
    {
        // Custom handling for MaxDropDownHeight changes can be added here
    }

    /// <summary>
    /// Called when the value of the <see cref="Command"/> property changes.
    /// Handles hooking and unhooking of command event handlers.
    /// </summary>
    /// <param name="oldValue">The old command associated with this dropdown.</param>
    /// <param name="newValue">The new command associated with this dropdown.</param>
    protected virtual void OnCommandChanged(ICommand? oldValue, ICommand? newValue)
    {
        // If the old command is not null, unhook its handlers
        UnhookCommand(oldValue, newValue);

        // Hook up the new command handlers
        HookUpCommand(oldValue, newValue);

        // Call this method to update command execution status
        CanExecuteChanged(); // May need to call this when changing the command parameter or target.
    }

    private void CanExecuteChanged()
    {
        if (Command != null)
        {
            // If a RoutedCommand.
            IsEnabled = Command is RoutedCommand command
                ? command.CanExecute(CommandParameter, CommandTarget)
                : Command.CanExecute(CommandParameter);
        }
    }

    /// <summary>
    /// Closes the drop down.
    /// </summary>
    private void CloseDropDown(bool isFocusOnButton)
    {
        if (IsOpen)
        {
            IsOpen = false;
        }

        ReleaseMouseCapture();

        if (isFocusOnButton && (Button != null))
        {
            Button.Focus();
        }
    }

    /// <summary>
    /// Invoked when the drop-down control is clicked.
    /// Raises the <see cref="Click"/> routed event and executes the associated command, if any.
    /// </summary>
    protected virtual void OnClick()
    {
        RaiseRoutedEvent(ClickEvent);
        RaiseCommand();
    }

    /// <summary>
    /// Raises routed events.
    /// </summary>
    private void RaiseRoutedEvent(RoutedEvent routedEvent)
    {
        var args = new RoutedEventArgs(routedEvent, this);
        RaiseEvent(args);
    }

    /// <summary>
    /// Raises the command's Execute event.
    /// </summary>
    private void RaiseCommand()
    {
        if (Command is null)
        {
            return;
        }

        if (Command is not RoutedCommand routedCommand)
        {
            Command.Execute(CommandParameter);
        }
        else
        {
            routedCommand.Execute(CommandParameter, CommandTarget);
        }
    }

    /// <summary>
    /// Unhooks a command from the Command property.
    /// </summary>
    /// <param name="oldCommand">The old command.</param>
    /// <param name="newCommand">The new command.</param>
    private void UnhookCommand(ICommand? oldCommand, ICommand? newCommand)
    {
        EventHandler handler = CanExecuteChanged;
        if (oldCommand != null)
            oldCommand.CanExecuteChanged -= handler;
    }

    /// <summary>
    /// Hooks up a command to the CanExecuteChanged event handler.
    /// </summary>
    /// <param name="oldCommand">The old command.</param>
    /// <param name="newCommand">The new command.</param>
    private void HookUpCommand(ICommand? oldCommand, ICommand? newCommand)
    {
        var handler = new EventHandler(CanExecuteChanged);
        _canExecuteChangedHandler = handler;

        if (newCommand != null)
        {
            newCommand.CanExecuteChanged += _canExecuteChangedHandler;
        }
    }

    private void Reposition()
    {
        ClearDropOppositeMethodInfo?.Invoke(_popup, null);
        RepositionMethodInfo?.Invoke(_popup, null);
        DropOpposite = GetDropOpposite();
    }

    private bool GetDropOpposite()
    {
        var dropOpposite = DropOppositePropertyInfo?.GetValue(_popup);
        return (bool?)dropOpposite ?? false;
    }

    private static bool IsMinWidthHeightValid(object value)
    {
        double v = (double)value;
        return !double.IsNaN(v) && v >= 0.0d && !double.IsPositiveInfinity(v);
    }

    private static bool IsWidthHeightValid(object value)
    {
        double v = (double)value;
        return (double.IsNaN(v)) || (v >= 0.0d && !double.IsPositiveInfinity(v));
    }

    private static bool IsMaxWidthHeightValid(object value)
    {
        double v = (double)value;
        return (!double.IsNaN(v) && v >= 0.0d);
    }

    private static void OnIsDefaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dropDown = (DropDown)d;
        dropDown.OnIsDefaultChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dropDown = (DropDown)d;
        dropDown.OnIsOpenChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private static void OnStaysOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dropDown = (DropDown)d;
        dropDown.OnStaysOpenChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private static void OnMaxDropDownHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dropDown = (DropDown)d;
        dropDown.OnMaxDropDownHeightChanged((double)e.OldValue, (double)e.NewValue);
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dropDown = (DropDown)d;
        dropDown.OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
    }

    private static ContextMenu? GetContextMenu(DependencyObject parent)
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child == null)
            {
                continue;
            }

            if (child is FrameworkElement children && children.ContextMenu != null && children.ContextMenu.IsOpen)
            {
                return children.ContextMenu;
            }
            else
            {
                var result = GetContextMenu(child);

                if (result != null)
                {
                    return result;
                }
            }
        }

        return null;
    }

    #endregion

    #region Event Handlers

    private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
    {
        if (!e.Handled && (e.Scope == null) && (e.Target == null))
        {
            e.Target = sender as DropDown;
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsOpen)
        {
            if (KeyboardUtilities.IsKeyModifyingPopupState(e))
            {
                IsOpen = true;
                // ContentPresenter items will get focus in Popup_Opened().
                e.Handled = true;
            }
        }
        else
        {
            if (KeyboardUtilities.IsKeyModifyingPopupState(e))
            {
                CloseDropDown(true);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CloseDropDown(true);
                e.Handled = true;
            }
        }
    }

    private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
    {
        if ((_popup != null) && !_popup.IsMouseDirectlyOver && !StaysOpen)
        {
            CloseDropDown(true);
        }
    }

    private void DropDownButton_Click(object sender, RoutedEventArgs e)
    {
        OnClick();
    }

    private void CanExecuteChanged(object? sender, EventArgs e)
    {
        CanExecuteChanged();
    }

    private void Popup_Opened(object? sender, EventArgs e)
    {
        _popup?.Dispatcher.BeginInvoke(() =>
        {
            var hwnd = ((HwndSource)PresentationSource.FromVisual(_popup.Child)).Handle;
            var rect = new RECT();

            if (User32.GetWindowRect(hwnd, ref rect))
            {
                _ = User32.SetWindowPos(hwnd, -2, rect.Left, rect.Top, (int)Width, (int)Height, 0);
            }

            DropOpposite = GetDropOpposite();

        }, System.Windows.Threading.DispatcherPriority.Render);

        // Set the focus on the content of the ContentPresenter.
        _contentPresenter?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
    }

    private void OnWindowLocationChanged(object? sender, EventArgs e)
    {
        if (_popup is null || !_popup.IsOpen)
        {
            return;
        }

        Reposition();
    }

    private void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_popup is null || _popupContent is null || !AllowResize)
        {
            return;
        }

        if (double.IsNaN(DropDownWidth))
        {
            DropDownWidth = _popupContent.DesiredSize.Width;
        }

        if (double.IsNaN(DropDownHeight))
        {
            DropDownHeight = _popupContent.DesiredSize.Height;
        }

        var xAdjust = DropDownWidth + e.HorizontalChange;
        var yAdjust = DropDownHeight + e.VerticalChange;

        if (xAdjust >= MinDropDownWidth && xAdjust <= MaxDropDownWidth)
        {
            DropDownWidth = xAdjust;
        }

        if (yAdjust >= MinDropDownHeight && yAdjust <= MaxDropDownHeight)
        {
            DropDownHeight = yAdjust;
        }

        Reposition();
    }

    #endregion
}