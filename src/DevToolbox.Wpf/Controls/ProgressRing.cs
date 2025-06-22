using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a control that displays a progress ring to indicate loading or processing activities.
/// The control can be customized with different colors and animation styles.
/// </summary>
[TemplateVisualState(GroupName = GroupActiveStates, Name = StateInactive)]
[TemplateVisualState(GroupName = GroupActiveStates, Name = StateActive)]
public class ProgressRing : Control
{
    #region Fields/Consts

    private const string GroupActiveStates = "ActiveStates";
    private const string StateInactive = "Inactive";
    private const string StateActive = "Active";

    /// <summary>
    /// Identifies the <see cref="ProgressRingColor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ProgressRingColorProperty =
        DependencyProperty.Register(nameof(ProgressRingColor), typeof(Brush), typeof(ProgressRing),
            new PropertyMetadata(Brushes.CornflowerBlue));

    /// <summary>
    /// Identifies the <see cref="IsActive"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(ProgressRing),
            new PropertyMetadata(default(bool), OnIsActiveChanged));

    /// <summary>
    /// Identifies the <see cref="ProgressRingStyle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ProgressRingStyleProperty =
        DependencyProperty.Register(nameof(ProgressRingStyle), typeof(ProgressRingStyle), typeof(ProgressRing),
            new PropertyMetadata(default(ProgressRingStyle), default));

    /// <summary>
    /// Identifies the <see cref="Size"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(double),
            typeof(ProgressRing),
            new PropertyMetadata(40.0));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the size (width and height) of the progress ring.
    /// </summary>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="ProgressRing"/> is showing progress.
    /// When set to true, the progress ring will display its active animation.
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the progress ring animation.
    /// </summary>
    public ProgressRingStyle ProgressRingStyle
    {
        get => (ProgressRingStyle)GetValue(ProgressRingStyleProperty);
        set => SetValue(ProgressRingStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the color of the progress ring.
    /// </summary>
    public Brush ProgressRingColor
    {
        get => (Brush)GetValue(ProgressRingColorProperty);
        set => SetValue(ProgressRingColorProperty, value);
    }

    #endregion

    static ProgressRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(typeof(ProgressRing)));
    }

    #region Methods Override

    /// <summary>
    /// Called when the control's template is applied.
    /// Initializes the visual state of the control based on the IsActive property.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        GotoCurrentState(false);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Transitions to the current visual state based on whether the progress ring is active.
    /// </summary>
    /// <param name="animate">Indicates whether the transition should be animated.</param>
    private void GotoCurrentState(bool animate)
    {
        VisualStateManager.GoToState(this, IsActive ? StateActive : StateInactive, animate);
    }

    /// <summary>
    /// Handles changes to the IsActive property.
    /// Updates the visual state of the control accordingly.
    /// </summary>
    /// <param name="oldValue">The previous value of the IsActive property.</param>
    /// <param name="newValue">The new value of the IsActive property.</param>
    private void OnIsActiveChanged(bool oldValue, bool newValue)
    {
        GotoCurrentState(true);
    }

    /// <summary>
    /// Callback method that is called when the IsActive property changes.
    /// </summary>
    /// <param name="d">The dependency object that changed.</param>
    /// <param name="e">The event data containing old and new values.</param>
    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var progressRing = (ProgressRing)d;
        progressRing.OnIsActiveChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    #endregion
}
