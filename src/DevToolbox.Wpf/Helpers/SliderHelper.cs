using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevToolbox.Wpf.Helpers;

/// <summary>
/// Provides attached properties and methods to enhance the functionality of WPF Sliders.
/// Specifically, it enables a feature to move the slider thumb to the point where the mouse is dragged.
/// </summary>
public class SliderHelper
{
    #region Fields/Consts

    /// <summary>
    /// A flag to indicate whether the mouse has been clicked within the slider.
    /// </summary>
    private static bool _clickedInSlider = false;

    /// <summary>
    /// An attached property that determines whether the slider should move to the point on drag.
    /// </summary>
    public static readonly DependencyProperty IsMoveToPointOnDragEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsMoveToPointOnDragEnabled",
            typeof(bool),
            typeof(SliderHelper),
            new PropertyMetadata(false, OnIsMoveToPointOnDragEnabledChanged));

    #endregion

    #region Methods

    /// <summary>
    /// Gets the value of the IsMoveToPointOnDragEnabled attached property for the specified object.
    /// </summary>
    /// <param name="obj">The target Slider to get the property value from.</param>
    /// <returns>True if moving to the point on drag is enabled; otherwise, false.</returns>
    public static bool GetIsMoveToPointOnDragEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsMoveToPointOnDragEnabledProperty);
    }

    /// <summary>
    /// Sets the value of the IsMoveToPointOnDragEnabled attached property for the specified object.
    /// </summary>
    /// <param name="obj">The target Slider to set the property value on.</param>
    /// <param name="value">True to enable moving to the point on drag; otherwise, false.</param>
    public static void SetIsMoveToPointOnDragEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsMoveToPointOnDragEnabledProperty, value);
    }

    /// <summary>
    /// Called when the IsMoveToPointOnDragEnabled property changes. 
    /// Subscribes or unsubscribes to mouse events based on the new property value.
    /// </summary>
    /// <param name="d">The dependency object that the property is attached to.</param>
    /// <param name="e">The event data containing the old and new property values.</param>
    private static void OnIsMoveToPointOnDragEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Slider slider) return;

        var isMoveToPointOnDragEnabled = (bool)e.NewValue;

        if (isMoveToPointOnDragEnabled)
        {
            slider.MouseMove += Slider_MouseMove;
            slider.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(Slider_PreviewMouseLeftButtonDown), true);
            slider.AddHandler(UIElement.PreviewMouseLeftButtonUpEvent, new RoutedEventHandler(Slider_PreviewMouseLeftButtonUp), true);
        }
        else
        {
            slider.MouseMove -= Slider_MouseMove;
            slider.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(Slider_PreviewMouseLeftButtonDown));
            slider.RemoveHandler(UIElement.PreviewMouseLeftButtonUpEvent, new RoutedEventHandler(Slider_PreviewMouseLeftButtonUp));
        }
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles the PreviewMouseLeftButtonDown event of the Slider. 
    /// Sets the _clickedInSlider flag to true when the left mouse button is pressed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private static void Slider_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        _clickedInSlider = true;
    }

    /// <summary>
    /// Handles the PreviewMouseLeftButtonUp event of the Slider. 
    /// Resets the _clickedInSlider flag to false when the left mouse button is released.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private static void Slider_PreviewMouseLeftButtonUp(object sender, RoutedEventArgs e)
    {
        _clickedInSlider = false;
    }

    /// <summary>
    /// Handles the MouseMove event of the Slider. 
    /// If the left mouse button is pressed and the mouse was clicked within the slider,
    /// raises the MouseLeftButtonDown event for the slider's thumb to move it to the cursor's position.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private static void Slider_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Slider slider) return;

        if (e.LeftButton == MouseButtonState.Pressed && _clickedInSlider)
        {
            if (slider.Template.FindName("PART_Track", slider) is not Track track) return;

            var mouseButtonEventArgs = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = e.Source
            };

            track.Thumb.RaiseEvent(mouseButtonEventArgs);
        }
    }

    #endregion
}
