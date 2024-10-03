using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a waiting indicator control that can display a message 
/// and show a progress ring with customizable styles and colors.
/// </summary>
public partial class WaitingIndicator : ContentControl
{
    #region Fields/Consts

    /// <summary>
    /// Identifies the <see cref="Message"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(object), typeof(WaitingIndicator), new PropertyMetadata(default));

    /// <summary>
    /// Identifies the <see cref="ProgressRingStyle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ProgressRingStyleProperty =
        DependencyProperty.Register(nameof(ProgressRingStyle), typeof(ProgressRingStyle), typeof(WaitingIndicator), new PropertyMetadata(ProgressRingStyle.ChasingDots));

    /// <summary>
    /// Identifies the <see cref="ProgressRingColor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ProgressRingColorProperty =
        DependencyProperty.Register(nameof(ProgressRingColor), typeof(Brush), typeof(WaitingIndicator), new PropertyMetadata(Brushes.CornflowerBlue));

    /// <summary>
    /// Identifies the <see cref="InProgress"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty InProgressProperty =
        DependencyProperty.Register(nameof(InProgress), typeof(bool), typeof(WaitingIndicator), new PropertyMetadata(default));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the message displayed by the waiting indicator.
    /// </summary>
    public object Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the progress ring.
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

    /// <summary>
    /// Gets or sets a value indicating whether the waiting indicator is currently in progress.
    /// </summary>
    public bool InProgress
    {
        get => (bool)GetValue(InProgressProperty);
        set => SetValue(InProgressProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes the <see cref="WaitingIndicator"/> class.
    /// </summary>
    static WaitingIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(WaitingIndicator), new FrameworkPropertyMetadata(typeof(WaitingIndicator)));
    }
}