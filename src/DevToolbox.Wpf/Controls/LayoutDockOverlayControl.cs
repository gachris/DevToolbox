using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A temporary overlay control that hosts a <see cref="LayoutDockItemsControl"/> when a docking button is clicked.
/// It raises <see cref="Opening"/> and <see cref="Closing"/> events and tracks its open state and associated dock side.
/// </summary>
public class LayoutDockOverlayControl : ContentControl
{
    #region Fields/Consts

    private static readonly DependencyPropertyKey IsOpenPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsOpen),
            typeof(bool),
            typeof(LayoutDockOverlayControl),
            new PropertyMetadata(false));

    private static readonly DependencyPropertyKey DockPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(Dock),
            typeof(Dock),
            typeof(LayoutDockOverlayControl),
            new FrameworkPropertyMetadata(Dock.Left));

    /// <summary>
    /// Identifies the <see cref="IsOpen"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsOpenProperty = IsOpenPropertyKey.DependencyProperty;

    /// <summary>
    /// Identifies the <see cref="Dock"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DockProperty = DockPropertyKey.DependencyProperty;

    /// <summary>
    /// Routed event that fires when the overlay is about to open.
    /// </summary>
    public static readonly RoutedEvent OpeningEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Opening),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(LayoutDockOverlayControl));

    /// <summary>
    /// Routed event that fires when the overlay is about to close.
    /// </summary>
    public static readonly RoutedEvent ClosingEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Closing),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(LayoutDockOverlayControl));

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether the overlay is currently open.
    /// </summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        private set => SetValue(IsOpenPropertyKey, value);
    }

    /// <summary>
    /// Gets the <see cref="Dock"/> position to which this overlay is aligned.
    /// </summary>
    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        private set => SetValue(DockPropertyKey, value);
    }

    /// <summary>
    /// Occurs when the overlay is opening.
    /// </summary>
    public event RoutedEventHandler Opening
    {
        add => AddHandler(OpeningEvent, value);
        remove => RemoveHandler(OpeningEvent, value);
    }

    /// <summary>
    /// Occurs when the overlay is closing.
    /// </summary>
    public event RoutedEventHandler Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    #endregion

    static LayoutDockOverlayControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LayoutDockOverlayControl),
            new FrameworkPropertyMetadata(typeof(LayoutDockOverlayControl)));
    }

    #region Methods

    /// <summary>
    /// Opens the overlay, sets its content to the target <see cref="LayoutDockItemsControl"/>, and raises <see cref="Opening"/>.
    /// </summary>
    /// <param name="dockingButton">The button that triggered the overlay.</param>
    public void Show(LayoutDockButton dockingButton)
    {
        if (dockingButton == null || IsOpen)
            return;

        IsOpen = true;
        Dock = dockingButton.Dock;
        if (dockingButton.AssociatedContainer != null)
            dockingButton.AssociatedContainer.SelectedIndex = dockingButton.AssociatedItemIndex;

        Content = dockingButton.AssociatedContainer;

        RaiseEvent(new RoutedEventArgs(OpeningEvent, this));
    }

    /// <summary>
    /// Closes the overlay, clears its content, and raises <see cref="Closing"/>.
    /// </summary>
    /// <param name="dockingButton">The button associated with this overlay.</param>
    public void Hide(LayoutDockButton dockingButton)
    {
        if (dockingButton == null || !IsOpen)
            return;

        Content = null;
        RaiseEvent(new RoutedEventArgs(ClosingEvent, this));
        IsOpen = false;
    }

    #endregion
}
