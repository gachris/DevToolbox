using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a speech balloon control that can display content in a pop-up style.
/// It provides properties for positioning and customizing the appearance of the balloon.
/// </summary>
public partial class SpeechBalloon : ContentControl
{
    #region Fields/Consts

    /// <summary> Identifies the <see cref="VerticalOffset"/> dependency property. </summary>
    /// <returns> The identifier for the <see cref="VerticalOffset"/> dependency property. </returns>
    public static readonly DependencyProperty VerticalOffsetProperty =
        Popup.VerticalOffsetProperty.AddOwner(typeof(SpeechBalloon));

    /// <summary> Identifies the <see cref="HorizontalOffset"/> dependency property. </summary>
    /// <returns> The identifier for the <see cref="HorizontalOffset"/> dependency property. </returns>
    public static readonly DependencyProperty HorizontalOffsetProperty =
        Popup.HorizontalOffsetProperty.AddOwner(typeof(SpeechBalloon));

    /// <summary> Identifies the <see cref="Placement"/> dependency property. </summary>
    /// <returns> The identifier for the <see cref="Placement"/> dependency property. </returns>
    public static readonly DependencyProperty PlacementProperty =
        Popup.PlacementProperty.AddOwner(typeof(SpeechBalloon));

    /// <summary> Identifies the <see cref="PlacementTarget"/> dependency property. </summary>
    /// <returns> The identifier for the <see cref="PlacementTarget"/> dependency property. </returns>
    public static readonly DependencyProperty PlacementTargetProperty =
        Popup.PlacementTargetProperty.AddOwner(typeof(SpeechBalloon));

    /// <summary> Identifies the <see cref="CornerRadius"/> dependency property. </summary>
    /// <returns> The identifier for the <see cref="CornerRadius"/> dependency property. </returns>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SpeechBalloon), new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary> Identifies the <see cref="SpeechBalloonPlacement"/> dependency property. </summary>
    /// <returns> The identifier for the <see cref="SpeechBalloonPlacement"/> dependency property. </returns>
    public static readonly DependencyProperty SpeechBalloonPlacementProperty =
        DependencyProperty.Register(nameof(SpeechBalloonPlacement), typeof(SpeechBalloonPlacement), typeof(SpeechBalloon),
            new FrameworkPropertyMetadata(SpeechBalloonPlacement.Auto, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary> Identifies the <see cref="SpeechBalloonHorizontalOffset"/> dependency property. </summary>
    /// <returns> The identifier for the <see cref="SpeechBalloonHorizontalOffset"/> dependency property. </returns>
    public static readonly DependencyProperty SpeechBalloonHorizontalOffsetProperty =
        DependencyProperty.Register(nameof(SpeechBalloonHorizontalOffset), typeof(double), typeof(SpeechBalloon), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary> Identifies the <see cref="SpeechBalloonVerticalOffset"/> dependency property. </summary>
    /// <returns> The identifier for the <see cref="SpeechBalloonVerticalOffset"/> dependency property. </returns>
    public static readonly DependencyProperty SpeechBalloonVerticalOffsetProperty =
        DependencyProperty.Register(nameof(SpeechBalloonVerticalOffset), typeof(double), typeof(SpeechBalloon), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary> Identifies the <see cref="DropOpposite"/> dependency property. </summary>
    /// <returns> The identifier for the <see cref="DropOpposite"/> dependency property. </returns>
    public static readonly DependencyProperty DropOppositeProperty =
        DependencyProperty.Register(nameof(DropOpposite), typeof(bool), typeof(SpeechBalloon), new FrameworkPropertyMetadata(false));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the orientation of the <see cref="SpeechBalloon"/> control
    /// when it opens, specifying how it interacts with screen boundaries.
    /// </summary>
    /// <returns>
    /// A <see cref="PlacementMode"/> enumeration value that determines the
    /// orientation of the <see cref="SpeechBalloon"/> when it opens. The default is <see cref="PlacementMode.Bottom"/>.
    /// </returns>
    [Bindable(true)]
    [Category("Layout")]
    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal distance between the target origin and the
    /// popup alignment point.
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
    /// Gets or sets the vertical distance between the target origin and the
    /// popup alignment point.
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
    /// Gets or sets the element relative to which the <see cref="SpeechBalloon"/>
    /// is positioned when it opens.
    /// </summary>
    /// <returns>
    /// The System.Windows.UIElement that is the logical parent of the <see cref="SpeechBalloon"/>.
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
    /// Gets or sets the corner radius of the speech balloon.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the placement option for the speech balloon.
    /// </summary>
    public SpeechBalloonPlacement SpeechBalloonPlacement
    {
        get => (SpeechBalloonPlacement)GetValue(SpeechBalloonPlacementProperty);
        set => SetValue(SpeechBalloonPlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal offset specific to the speech balloon.
    /// </summary>
    public double SpeechBalloonHorizontalOffset
    {
        get => (double)GetValue(SpeechBalloonHorizontalOffsetProperty);
        set => SetValue(SpeechBalloonHorizontalOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical offset specific to the speech balloon.
    /// </summary>
    public double SpeechBalloonVerticalOffset
    {
        get => (double)GetValue(SpeechBalloonVerticalOffsetProperty);
        set => SetValue(SpeechBalloonVerticalOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the speech balloon should drop opposite to the target element.
    /// </summary>
    public bool DropOpposite
    {
        get => (bool)GetValue(DropOppositeProperty);
        set => SetValue(DropOppositeProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes the <see cref="SpeechBalloon"/> class and overrides the default style key.
    /// </summary>
    static SpeechBalloon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SpeechBalloon), new FrameworkPropertyMetadata(typeof(SpeechBalloon)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeechBalloon"/> class.
    /// </summary>
    public SpeechBalloon()
    {
    }

    #region Methods

    /// <summary>
    /// Validates the width or height value for the balloon.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>
    /// True if the value is valid; otherwise, false.
    /// </returns>
    private static bool IsWidthHeightValid(object value)
    {
        double v = (double)value;
        return (double.IsNaN(v)) || (v >= 0.0d && !Double.IsPositiveInfinity(v));
    }

    #endregion
}
