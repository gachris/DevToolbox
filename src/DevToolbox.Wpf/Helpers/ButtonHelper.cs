using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevToolbox.Wpf.Helpers;

/// <summary>
/// Provides attached properties and methods to enable showing a context menu
/// on a Button when it is clicked with the left mouse button.
/// </summary>
public class ButtonHelper
{
    #region Fields/Consts

    /// <summary>
    /// Dependency property that determines whether to show the context menu on left click.
    /// </summary>
    public static readonly DependencyProperty ShowContextMenuOnLeftClickProperty =
        DependencyProperty.RegisterAttached(
            "ShowContextMenuOnLeftClick",
            typeof(bool),
            typeof(ButtonHelper),
            new FrameworkPropertyMetadata(OnShowContextMenuOnLeftClickChanged));

    #endregion

    #region Methods

    /// <summary>
    /// Gets the value of the ShowContextMenuOnLeftClick attached property.
    /// </summary>
    /// <param name="obj">The target object to get the property value from.</param>
    /// <returns>True if the context menu should be shown on left click; otherwise, false.</returns>
    public static bool GetShowContextMenuOnLeftClick(DependencyObject obj)
    {
        return (bool)obj.GetValue(ShowContextMenuOnLeftClickProperty);
    }

    /// <summary>
    /// Sets the value of the ShowContextMenuOnLeftClick attached property.
    /// </summary>
    /// <param name="obj">The target object to set the property value on.</param>
    /// <param name="value">True to show the context menu on left click; otherwise, false.</param>
    public static void SetShowContextMenuOnLeftClick(DependencyObject obj, bool value)
    {
        obj.SetValue(ShowContextMenuOnLeftClickProperty, value);
    }

    /// <summary>
    /// Called when the ShowContextMenuOnLeftClick property changes.
    /// Subscribes or unsubscribes the mouse down event handler based on the property's value.
    /// </summary>
    /// <param name="d">The dependency object that the property is attached to.</param>
    /// <param name="e">The event data containing the old and new property values.</param>
    private static void OnShowContextMenuOnLeftClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button button) return;

        if ((bool)e.NewValue)
            button.PreviewMouseDown += OnMouseDown;
        else
            button.PreviewMouseDown -= OnMouseDown;
    }

    /// <summary>
    /// Displays the context menu associated with the button.
    /// </summary>
    /// <param name="button">The button whose context menu is to be shown.</param>
    private static void ShowContextMenu(Button button)
    {
        if (button.ContextMenu != null)
        {
            // Create the ContextMenuEventArgs manually
            var contextMenuEventArgs = (ContextMenuEventArgs)FormatterServices.GetUninitializedObject(typeof(ContextMenuEventArgs));
            contextMenuEventArgs.RoutedEvent = FrameworkElement.ContextMenuOpeningEvent;
            contextMenuEventArgs.Source = button;

            // Raise the ContextMenuOpening event
            button.RaiseEvent(contextMenuEventArgs);

            button.ContextMenu.Placement = ContextMenuService.GetPlacement(button);
            var placementTarget = ContextMenuService.GetPlacementTarget(button);
            placementTarget ??= button;
            button.ContextMenu.PlacementTarget = placementTarget;

            // Open the context menu
            button.ContextMenu.IsOpen = true;
        }
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles the mouse down events for the button to show the context menu on left click.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The mouse button event data.</param>
    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button button && e.ChangedButton == MouseButton.Left)
            ShowContextMenu(button);
    }

    #endregion
}
