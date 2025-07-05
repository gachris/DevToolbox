using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A button used within a <see cref="DockingButtonGroupControl"/>,
/// representing a docking action (e.g., dock left, dock right, auto-hide).
/// </summary>
public class DockingButton : Button
{
    #region Fields/Consts

    /// <summary>
    /// Internal command to toggle the overlay visibility for docking hints.
    /// </summary>
    private static readonly RoutedUICommand _showHideOverlay =
        new(nameof(ShowHideOverlayCommand), nameof(ShowHideOverlayCommand), typeof(DockingButton));

    private DockingButtonGroupItem? _parentGroupItem;

    /// <summary>
    /// Identifies the <see cref="Icon"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon), typeof(ImageSource), typeof(DockingButton), new FrameworkPropertyMetadata(default(ImageSource)));

    /// <summary>
    /// Identifies the read-only <see cref="Dock"/> dependency property.
    /// </summary>
    private static readonly DependencyPropertyKey DockPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(Dock), typeof(Dock), typeof(DockingButton), new FrameworkPropertyMetadata(default(Dock)));

    /// <summary>
    /// The <see cref="Dock"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DockProperty = DockPropertyKey.DependencyProperty;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the command used to show or hide the docking overlay.
    /// </summary>
    public static RoutedUICommand ShowHideOverlayCommand => _showHideOverlay;

    /// <summary>
    /// Gets the docking direction associated with this button (Left, Right, Top, Bottom).
    /// </summary>
    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        private set => SetValue(DockPropertyKey, value);
    }

    /// <summary>
    /// Gets or sets the image icon displayed on the button.
    /// </summary>
    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the index of this button within its parent group.
    /// </summary>
    public int AssociatedItemIndex { get; internal set; }

    /// <summary>
    /// Gets or sets the <see cref="DockableControl"/> that this button will act upon.
    /// </summary>
    public DockableControl? AssociatedContainer { get; internal set; }

    /// <summary>
    /// Gets or sets the parent group item that contains this button, which provides the <see cref="Dock"/> value.
    /// </summary>
    public DockingButtonGroupItem? ParentGroupItem
    {
        get => _parentGroupItem;
        internal set
        {
            _parentGroupItem = value;
            Dock = _parentGroupItem?.Dock ?? default;
        }
    }

    #endregion

    static DockingButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DockingButton),
            new FrameworkPropertyMetadata(typeof(DockingButton)));
    }
}
