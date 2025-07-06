using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A button used within a <see cref="LayoutDockButtonGroupControl"/>,
/// representing a docking action (e.g., dock left, dock right, auto-hide).
/// </summary>
public class LayoutDockButton : Button
{
    #region Fields/Consts

    private LayoutDockButtonGroupItem? _parentGroupItem;

    /// <summary>
    /// Identifies the <see cref="Icon"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon), typeof(ImageSource), typeof(LayoutDockButton), new FrameworkPropertyMetadata(default(ImageSource)));

    /// <summary>
    /// Identifies the read-only <see cref="Dock"/> dependency property.
    /// </summary>
    private static readonly DependencyPropertyKey DockPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(Dock), typeof(Dock), typeof(LayoutDockButton), new FrameworkPropertyMetadata(default(Dock)));

    /// <summary>
    /// The <see cref="Dock"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DockProperty = DockPropertyKey.DependencyProperty;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the command used to show or hide the docking overlay.
    /// </summary>
    public static RoutedUICommand ShowHideOverlayCommand { get; } = new(nameof(ShowHideOverlayCommand), nameof(ShowHideOverlayCommand), typeof(LayoutDockButton));

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
    /// Gets or sets the <see cref="LayoutDockItemsControl"/> that this button will act upon.
    /// </summary>
    public LayoutDockItemsControl? AssociatedContainer { get; internal set; }

    /// <summary>
    /// Gets or sets the parent group item that contains this button, which provides the <see cref="Dock"/> value.
    /// </summary>
    public LayoutDockButtonGroupItem? ParentGroupItem
    {
        get => _parentGroupItem;
        internal set
        {
            _parentGroupItem = value;
            Dock = _parentGroupItem?.Dock ?? default;
        }
    }

    #endregion

    static LayoutDockButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LayoutDockButton),
            new FrameworkPropertyMetadata(typeof(LayoutDockButton)));
    }
}
