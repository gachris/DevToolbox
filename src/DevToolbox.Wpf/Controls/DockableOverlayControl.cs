using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class DockableOverlayControl : ContentControl
{
    #region Fields/Consts

    private static readonly DependencyPropertyKey IsOpenPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsOpen), typeof(bool), typeof(DockableOverlayControl), new PropertyMetadata(default));

    private static readonly DependencyPropertyKey DockPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Dock), typeof(Dock), typeof(DockableOverlayControl), new FrameworkPropertyMetadata(default(Dock)));

    public static readonly RoutedEvent ClosingEvent =
        EventManager.RegisterRoutedEvent(nameof(Closing), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DockableOverlayControl));
    public static readonly RoutedEvent OpeningEvent =
        EventManager.RegisterRoutedEvent(nameof(Opening), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DockableOverlayControl));

    public static readonly DependencyProperty IsOpenProperty = IsOpenPropertyKey.DependencyProperty;
    public static readonly DependencyProperty DockProperty = DockPropertyKey.DependencyProperty;

    #endregion

    #region Properties

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        private set => SetValue(IsOpenPropertyKey, value);
    }

    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        private set => SetValue(DockPropertyKey, value);
    }

    public event RoutedEventHandler Opening
    {
        add => AddHandler(OpeningEvent, value);
        remove => RemoveHandler(OpeningEvent, value);
    }

    public event RoutedEventHandler Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    #endregion

    static DockableOverlayControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DockableOverlayControl), new FrameworkPropertyMetadata(typeof(DockableOverlayControl)));
    }

    #region Methods

    /// <summary>
    /// Show tampoary control attached to current docking buttno
    /// </summary>
    public void Show(DockingButton dockingButton)
    {
        if (dockingButton is not null && !IsOpen)
        {
            IsOpen = true;
            Dock = dockingButton.Dock;

            if (dockingButton.AssociatedContainer is not null)
            {
                dockingButton.AssociatedContainer.SelectedIndex = dockingButton.AssociatedItemIdex;
            }

            Content = dockingButton.AssociatedContainer;

            RoutedEventArgs routedEventArg = new()
            {
                RoutedEvent = OpeningEvent,
                Source = this
            };

            RaiseEvent(routedEventArg);
        }
    }

    /// <summary>
    /// Hide temporay control and reset current docking button
    /// </summary>
    public void Hide(DockingButton dockingButton)
    {
        if (dockingButton is not null && IsOpen)
        {
            Content = null;

            RoutedEventArgs routedEventArg = new()
            {
                RoutedEvent = ClosingEvent,
                Source = this
            };

            RaiseEvent(routedEventArg);
        }

        IsOpen = false;
    }

    #endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member